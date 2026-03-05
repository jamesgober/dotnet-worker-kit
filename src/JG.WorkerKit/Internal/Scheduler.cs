using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace JG.WorkerKit.Internal;

/// <summary>
/// Internal implementation of the scheduler.
/// </summary>
internal sealed class Scheduler : IScheduler, ISchedulerInternal
{
    private static readonly Action<ILogger, string, Exception?> _jobError = LoggerMessage.Define<string>(
        LogLevel.Error, new EventId(100, "ScheduledJobError"), "Error executing scheduled job {JobType}");

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<Scheduler> _logger;
    private readonly ConcurrentBag<ScheduleEntry> _entries;

    /// <summary>
    /// Initializes a new instance of the <see cref="Scheduler"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="entries">The schedule entries.</param>
    public Scheduler(IServiceProvider serviceProvider, ILogger<Scheduler> logger, IEnumerable<ScheduleEntry> entries)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _entries = new ConcurrentBag<ScheduleEntry>(entries);
    }

    /// <inheritdoc/>
    public void AddRecurringJob<TJob>(string cronExpression) where TJob : IScheduledTask
    {
        _entries.Add(new ScheduleEntry(typeof(TJob), cronExpression));
    }

    /// <inheritdoc/>
    public void AddRecurringJob<TJob>(TimeSpan interval) where TJob : IScheduledTask
    {
        _entries.Add(new ScheduleEntry(typeof(TJob), interval));
    }

    /// <summary>
    /// Starts the scheduler.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the operation.</returns>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        var tasks = new List<Task>();

        foreach (var entry in _entries)
        {
            if (entry.IsCron)
            {
                tasks.Add(StartCronJobAsync(entry, cancellationToken));
            }
            else
            {
                tasks.Add(StartIntervalJobAsync(entry, cancellationToken));
            }
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    private async Task StartCronJobAsync(ScheduleEntry entry, CancellationToken cancellationToken)
    {
        var cron = new CronExpression(entry.CronExpression!);
        var nextRun = cron.GetNextOccurrence(DateTime.UtcNow);

        while (!cancellationToken.IsCancellationRequested)
        {
            var delay = nextRun - DateTime.UtcNow;
            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }

            if (!cancellationToken.IsCancellationRequested)
            {
                await ExecuteJobAsync(entry.JobType, cancellationToken).ConfigureAwait(false);
            }

            nextRun = cron.GetNextOccurrence(DateTime.UtcNow);
        }
    }

    private async Task StartIntervalJobAsync(ScheduleEntry entry, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(entry.Interval, cancellationToken).ConfigureAwait(false);

            if (!cancellationToken.IsCancellationRequested)
            {
                await ExecuteJobAsync(entry.JobType, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private async Task ExecuteJobAsync(Type jobType, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var job = (IScheduledTask)scope.ServiceProvider.GetRequiredService(jobType);
            await job.ExecuteAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _jobError(_logger, jobType.Name, ex);
        }
    }
}
