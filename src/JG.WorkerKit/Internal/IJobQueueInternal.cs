namespace JG.WorkerKit.Internal;

/// <summary>
/// Internal interface for job queue operations.
/// </summary>
internal interface IJobQueueInternal
{
    /// <summary>
    /// Dequeues a job item from the queue.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The dequeued job item, or null if the queue is complete.</returns>
    ValueTask<JobItem?> DequeueAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks the queue as complete for writing.
    /// </summary>
    void Complete();
}
