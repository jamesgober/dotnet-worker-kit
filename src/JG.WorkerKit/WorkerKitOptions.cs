namespace JG.WorkerKit;

/// <summary>
/// Options for configuring the WorkerKit.
/// </summary>
public sealed class WorkerKitOptions
{
    /// <summary>
    /// Gets or sets the number of worker threads.
    /// </summary>
    public int WorkerCount { get; set; } = 2;

    /// <summary>
    /// Gets or sets the capacity of the job queue.
    /// </summary>
    public int QueueCapacity { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the shutdown timeout.
    /// </summary>
    public TimeSpan ShutdownTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the default retry policy.
    /// </summary>
    public RetryPolicy DefaultRetryPolicy { get; set; } = RetryPolicy.Exponential(maxRetries: 3);
}
