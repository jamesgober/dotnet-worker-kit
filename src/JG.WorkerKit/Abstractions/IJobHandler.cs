namespace JG.WorkerKit;

/// <summary>
/// Defines a handler for a job with typed data.
/// </summary>
/// <typeparam name="TData">The type of the job data.</typeparam>
public interface IJobHandler<TData>
{
    /// <summary>
    /// Handles the job execution.
    /// </summary>
    /// <param name="data">The job data.</param>
    /// <param name="context">The job execution context.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the job execution.</returns>
    ValueTask HandleAsync(TData data, JobContext context, CancellationToken cancellationToken = default);
}
