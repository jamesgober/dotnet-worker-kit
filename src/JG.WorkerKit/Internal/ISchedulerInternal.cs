namespace JG.WorkerKit.Internal;

/// <summary>
/// Internal interface for scheduler operations.
/// </summary>
internal interface ISchedulerInternal
{
    /// <summary>
    /// Starts the scheduler.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the operation.</returns>
    Task StartAsync(CancellationToken cancellationToken = default);
}
