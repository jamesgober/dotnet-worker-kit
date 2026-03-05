namespace JG.WorkerKit;

/// <summary>
/// Represents a scheduler for recurring jobs.
/// </summary>
public interface IScheduler
{
    /// <summary>
    /// Adds a recurring job with a cron expression.
    /// </summary>
    /// <typeparam name="TJob">The type of the scheduled task.</typeparam>
    /// <param name="cronExpression">The cron expression defining the schedule.</param>
    void AddRecurringJob<TJob>(string cronExpression) where TJob : IScheduledTask;

    /// <summary>
    /// Adds a recurring job with a time interval.
    /// </summary>
    /// <typeparam name="TJob">The type of the scheduled task.</typeparam>
    /// <param name="interval">The interval between executions.</param>
    void AddRecurringJob<TJob>(TimeSpan interval) where TJob : IScheduledTask;
}
