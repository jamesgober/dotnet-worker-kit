namespace JG.WorkerKit;

/// <summary>
/// Represents a job queue for enqueuing background work items.
/// </summary>
public interface IJobQueue
{
    /// <summary>
    /// Enqueues a job for asynchronous execution.
    /// </summary>
    /// <typeparam name="TJob">The type of the job handler.</typeparam>
    /// <typeparam name="TData">The type of the job data.</typeparam>
    /// <param name="data">The data to pass to the job handler.</param>
    /// <param name="options">Optional job options.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the enqueue operation.</returns>
    ValueTask EnqueueAsync<TJob, TData>(TData data, JobOptions? options = null, CancellationToken cancellationToken = default)
        where TJob : IJobHandler<TData>;
}
