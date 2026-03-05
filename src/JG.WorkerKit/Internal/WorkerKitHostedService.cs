using System.Threading.Channels;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace JG.WorkerKit.Internal;

/// <summary>
/// The hosted service for WorkerKit.
/// </summary>
internal sealed class WorkerKitHostedService : IHostedService
{
    private static readonly Action<ILogger, int, Exception?> _starting = LoggerMessage.Define<int>(
        LogLevel.Information, new EventId(1, "Starting"), "Starting WorkerKit with {WorkerCount} workers");

    private static readonly Action<ILogger, Exception?> _stopping = LoggerMessage.Define(
        LogLevel.Information, new EventId(2, "Stopping"), "Stopping WorkerKit");

    private static readonly Action<ILogger, TimeSpan, Exception?> _shutdownTimeout = LoggerMessage.Define<TimeSpan>(
        LogLevel.Warning, new EventId(3, "ShutdownTimeout"), "WorkerKit shutdown timed out after {Timeout}");

    private static readonly Action<ILogger, Exception?> _shutdownGraceful = LoggerMessage.Define(
        LogLevel.Information, new EventId(4, "ShutdownGraceful"), "WorkerKit stopped gracefully");

    private static readonly Action<ILogger, int, Exception?> _workerStarted = LoggerMessage.Define<int>(
        LogLevel.Debug, new EventId(5, "WorkerStarted"), "Worker {WorkerId} started");

    private static readonly Action<ILogger, int, Exception?> _workerStopped = LoggerMessage.Define<int>(
        LogLevel.Debug, new EventId(6, "WorkerStopped"), "Worker {WorkerId} stopped");

    private static readonly Action<ILogger, int, Exception?> _workerError = LoggerMessage.Define<int>(
        LogLevel.Error, new EventId(7, "WorkerError"), "Error in worker {WorkerId}");

    private static readonly Action<ILogger, string, int, Exception?> _jobTimeout = LoggerMessage.Define<string, int>(
        LogLevel.Warning, new EventId(8, "JobTimeout"), "Job {JobType} timed out on attempt {Attempt}");

    private static readonly Action<ILogger, string, int, Exception?> _jobError = LoggerMessage.Define<string, int>(
        LogLevel.Error, new EventId(9, "JobError"), "Error processing job {JobType} on attempt {Attempt}");

    private static readonly Action<ILogger, string, Guid, Exception?> _deadLetter = LoggerMessage.Define<string, Guid>(
        LogLevel.Error, new EventId(10, "DeadLetter"), "Job {JobType} with correlation ID {CorrelationId} moved to dead letter");

    private readonly IJobQueue _jobQueue;
    private readonly IScheduler _scheduler;
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptions<WorkerKitOptions> _options;
    private readonly ILogger<WorkerKitHostedService> _logger;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly IJobMiddleware[] _middlewares;
    private readonly List<Task> _workerTasks = new();
    private readonly CancellationTokenSource _shutdownCts = new();
    private readonly ConcurrentDictionary<Type, Delegate> _handlerCache = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkerKitHostedService"/> class.
    /// </summary>
    /// <param name="jobQueue">The job queue.</param>
    /// <param name="scheduler">The scheduler.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="options">The options.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="lifetime">The host application lifetime.</param>
    /// <param name="middlewares">The job middlewares.</param>
    public WorkerKitHostedService(
        IJobQueue jobQueue,
        IScheduler scheduler,
        IServiceProvider serviceProvider,
        IOptions<WorkerKitOptions> options,
        ILogger<WorkerKitHostedService> logger,
        IHostApplicationLifetime lifetime,
        IEnumerable<IJobMiddleware> middlewares)
    {
        _jobQueue = jobQueue;
        _scheduler = scheduler;
        _serviceProvider = serviceProvider;
        _options = options;
        _logger = logger;
        _lifetime = lifetime;
        _middlewares = middlewares.Reverse().ToArray();
    }

    /// <inheritdoc/>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _starting(_logger, _options.Value.WorkerCount, null);

        // Start scheduler
        if (_scheduler is ISchedulerInternal schedulerInternal)
        {
            _ = schedulerInternal.StartAsync(_shutdownCts.Token);
        }

        // Start workers
        for (int i = 0; i < _options.Value.WorkerCount; i++)
        {
            _workerTasks.Add(Task.Run(() => WorkerLoopAsync(i, _shutdownCts.Token), _shutdownCts.Token));
        }

        _lifetime.ApplicationStopping.Register(() => _shutdownCts.Cancel());
    }

    /// <inheritdoc/>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _stopping(_logger, null);

        _shutdownCts.Cancel();

        // Complete the job queue
        if (_jobQueue is IJobQueueInternal queueInternal)
        {
            queueInternal.Complete();
        }

        // Wait for workers to finish with timeout
        var timeoutTask = Task.Delay(_options.Value.ShutdownTimeout, cancellationToken);
        var workersTask = Task.WhenAll(_workerTasks);

        await Task.WhenAny(workersTask, timeoutTask).ConfigureAwait(false);

        if (!workersTask.IsCompleted)
        {
            _shutdownTimeout(_logger, _options.Value.ShutdownTimeout, null);
        }
        else
        {
            _shutdownGraceful(_logger, null);
        }
    }

    private async Task WorkerLoopAsync(int workerId, CancellationToken cancellationToken)
    {
        _workerStarted(_logger, workerId, null);

        if (_jobQueue is not IJobQueueInternal queueInternal)
        {
            return;
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var item = await queueInternal.DequeueAsync(cancellationToken).ConfigureAwait(false);
                if (item is not { } jobItem)
                {
                    break; // Queue completed
                }

                await ProcessJobAsync(jobItem, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _workerError(_logger, workerId, ex);
            }
        }

        _workerStopped(_logger, workerId, null);
    }

    private async Task ProcessJobAsync(JobItem item, CancellationToken cancellationToken)
    {
        var retryPolicy = item.Options.MaxRetries > 0 ? new RetryPolicy(item.Options.MaxRetries, item.Options.RetryDelay, TimeSpan.FromSeconds(1), TimeSpan.FromMinutes(1)) : _options.Value.DefaultRetryPolicy;
        var attempt = 0;
        var deadLetter = false;

        while (attempt <= retryPolicy.MaxRetries && !cancellationToken.IsCancellationRequested)
        {
            attempt++;
            var context = new JobContext(attempt, item.EnqueueTime, item.CorrelationId);

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService(item.JobType);

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                if (item.Options.Timeout.HasValue)
                {
                    cts.CancelAfter(item.Options.Timeout.Value);
                }

                Func<ValueTask> next = () => ExecuteJobAsync(handler, item, context, cts.Token);

                foreach (var middleware in _middlewares)
                {
                    var current = next;
                    next = () => middleware.InvokeAsync(context, current);
                }

                await next().ConfigureAwait(false);

                // Success
                break;
            }
            catch (OperationCanceledException) when (item.Options.Timeout.HasValue)
            {
                _jobTimeout(_logger, item.JobType.Name, attempt, null);
                if (attempt > retryPolicy.MaxRetries)
                {
                    deadLetter = true;
                }
            }
            catch (Exception ex)
            {
                _jobError(_logger, item.JobType.Name, attempt, ex);
                if (attempt > retryPolicy.MaxRetries)
                {
                    deadLetter = true;
                }
            }

            if (attempt <= retryPolicy.MaxRetries && !deadLetter)
            {
                var delay = CalculateDelay(retryPolicy, attempt);
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }
        }

        if (deadLetter && item.Options.EnableDeadLetter)
        {
            _deadLetter(_logger, item.JobType.Name, item.CorrelationId, null);
        }
    }

    private async ValueTask ExecuteJobAsync(object handler, JobItem item, JobContext context, CancellationToken cancellationToken)
    {
        var handlerType = handler.GetType();

        if (!_handlerCache.TryGetValue(handlerType, out var cachedDelegate))
        {
            var method = handlerType.GetMethod(nameof(IJobHandler<object>.HandleAsync));
            if (method == null)
            {
                throw new InvalidOperationException($"Job handler {handlerType.Name} does not have HandleAsync method");
            }

            var dataType = item.Data.GetType();
            var invokeDelegate = method.CreateDelegate(typeof(Func<,,,,>).MakeGenericType(
                handlerType, dataType, typeof(JobContext), typeof(CancellationToken), typeof(ValueTask)));

            cachedDelegate = invokeDelegate;
            _handlerCache.TryAdd(handlerType, cachedDelegate);
        }

        await ((dynamic)cachedDelegate).Invoke(handler, item.Data, context, cancellationToken);
    }

    private static TimeSpan CalculateDelay(RetryPolicy policy, int attempt)
    {
        var baseDelay = policy.InitialDelay;

        switch (policy.DelayStrategy)
        {
            case RetryDelayStrategy.Fixed:
                return baseDelay;
            case RetryDelayStrategy.Linear:
                return TimeSpan.FromTicks(baseDelay.Ticks * attempt);
            case RetryDelayStrategy.ExponentialWithJitter:
                var exponential = TimeSpan.FromTicks((long)(baseDelay.Ticks * Math.Pow(2, attempt - 1)));
                var jitter = Random.Shared.NextDouble() * 0.1 * exponential.TotalMilliseconds;
                var delay = exponential + TimeSpan.FromMilliseconds(jitter);
                return delay > policy.MaxDelay ? policy.MaxDelay : delay;
            default:
                return baseDelay;
        }
    }
}
