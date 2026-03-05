namespace JG.WorkerKit;

/// <summary>
/// Options for a job.
/// </summary>
public sealed class JobOptions
{
    /// <summary>
    /// Gets the default job options.
    /// </summary>
    public static readonly JobOptions Default = new();

    /// <summary>
    /// Gets or sets the job priority.
    /// </summary>
    public JobPriority Priority { get; set; } = JobPriority.Normal;

    /// <summary>
    /// Gets or sets the maximum number of retries.
    /// </summary>
    public int MaxRetries { get; set; }

    /// <summary>
    /// Gets or sets the job timeout.
    /// </summary>
    public TimeSpan? Timeout { get; set; }

    /// <summary>
    /// Gets or sets the retry delay strategy.
    /// </summary>
    public RetryDelayStrategy RetryDelay { get; set; }

    /// <summary>
    /// Gets or sets whether to enable dead letter handling.
    /// </summary>
    public bool EnableDeadLetter { get; set; } = true;
}
