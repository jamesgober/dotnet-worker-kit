namespace JG.WorkerKit;

/// <summary>
/// Defines a scheduled task that can be executed on a recurring basis.
/// </summary>
public interface IScheduledTask
{
    /// <summary>
    /// Executes the scheduled task.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the task execution.</returns>
    ValueTask ExecuteAsync(CancellationToken cancellationToken = default);
}
