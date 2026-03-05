namespace JG.WorkerKit;

/// <summary>
/// Represents a schedule entry for a recurring job.
/// </summary>
public sealed class ScheduleEntry
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduleEntry"/> class.
    /// </summary>
    /// <param name="jobType">The job type.</param>
    /// <param name="cronExpression">The cron expression.</param>
    public ScheduleEntry(Type jobType, string cronExpression)
    {
        ArgumentNullException.ThrowIfNull(jobType);
        ArgumentNullException.ThrowIfNull(cronExpression);

        JobType = jobType;
        CronExpression = cronExpression;
        IsCron = true;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduleEntry"/> class.
    /// </summary>
    /// <param name="jobType">The job type.</param>
    /// <param name="interval">The interval.</param>
    public ScheduleEntry(Type jobType, TimeSpan interval)
    {
        ArgumentNullException.ThrowIfNull(jobType);

        JobType = jobType;
        Interval = interval;
        IsCron = false;
    }

    /// <summary>
    /// Gets the job type.
    /// </summary>
    public Type JobType { get; }

    /// <summary>
    /// Gets the cron expression.
    /// </summary>
    public string? CronExpression { get; }

    /// <summary>
    /// Gets the interval.
    /// </summary>
    public TimeSpan Interval { get; }

    /// <summary>
    /// Gets a value indicating whether this is a cron-based schedule.
    /// </summary>
    public bool IsCron { get; }
}
