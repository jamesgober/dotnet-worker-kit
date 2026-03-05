using System.Threading.Channels;
using Microsoft.Extensions.Options;

namespace JG.WorkerKit.Internal;

/// <summary>
/// Internal implementation of the job queue.
/// </summary>
internal sealed class JobQueue : IJobQueue, IJobQueueInternal
{
    private readonly Channel<JobItem> _criticalChannel;
    private readonly Channel<JobItem> _normalChannel;
    private readonly Channel<JobItem> _lowChannel;

    /// <summary>
    /// Initializes a new instance of the <see cref="JobQueue"/> class.
    /// </summary>
    /// <param name="options">The worker kit options.</param>
    public JobQueue(IOptions<WorkerKitOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var capacity = options.Value.QueueCapacity;
        _criticalChannel = Channel.CreateBounded<JobItem>(capacity);
        _normalChannel = Channel.CreateBounded<JobItem>(capacity);
        _lowChannel = Channel.CreateBounded<JobItem>(capacity);
    }

    /// <inheritdoc/>
    public async ValueTask EnqueueAsync<TJob, TData>(TData data, JobOptions? options = null, CancellationToken cancellationToken = default)
        where TJob : IJobHandler<TData>
    {
        ArgumentNullException.ThrowIfNull(data);

        options ??= JobOptions.Default;
        var item = new JobItem(typeof(TJob), data, options, Guid.NewGuid(), DateTime.UtcNow);

        var channel = options.Priority switch
        {
            JobPriority.Critical => _criticalChannel,
            JobPriority.Normal => _normalChannel,
            JobPriority.Low => _lowChannel,
            _ => _normalChannel
        };

        await channel.Writer.WriteAsync(item, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Dequeues a job item.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The dequeued item or null if the queue is completed.</returns>
    public async ValueTask<JobItem?> DequeueAsync(CancellationToken cancellationToken = default)
    {
        // Try critical first
        if (_criticalChannel.Reader.TryRead(out var item))
        {
            return item;
        }

        // Then normal
        if (_normalChannel.Reader.TryRead(out item))
        {
            return item;
        }

        // Then low
        if (_lowChannel.Reader.TryRead(out item))
        {
            return item;
        }

        // If none available, wait for any
        var criticalTask = _criticalChannel.Reader.WaitToReadAsync(cancellationToken).AsTask();
        var normalTask = _normalChannel.Reader.WaitToReadAsync(cancellationToken).AsTask();
        var lowTask = _lowChannel.Reader.WaitToReadAsync(cancellationToken).AsTask();

        var completedTask = await Task.WhenAny(criticalTask, normalTask, lowTask).ConfigureAwait(false);

        if (completedTask == criticalTask && criticalTask.Result)
        {
            _criticalChannel.Reader.TryRead(out item);
            return item;
        }
        else if (completedTask == normalTask && normalTask.Result)
        {
            _normalChannel.Reader.TryRead(out item);
            return item;
        }
        else if (completedTask == lowTask && lowTask.Result)
        {
            _lowChannel.Reader.TryRead(out item);
            return item;
        }

        return null;
    }

    /// <summary>
    /// Completes the channels for writing.
    /// </summary>
    public void Complete()
    {
        _criticalChannel.Writer.Complete();
        _normalChannel.Writer.Complete();
        _lowChannel.Writer.Complete();
    }
}
