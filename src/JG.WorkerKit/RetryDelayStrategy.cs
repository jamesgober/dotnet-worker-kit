namespace JG.WorkerKit;

/// <summary>
/// Represents the retry delay strategy.
/// </summary>
public enum RetryDelayStrategy
{
    /// <summary>
    /// Fixed delay.
    /// </summary>
    Fixed,

    /// <summary>
    /// Linear backoff.
    /// </summary>
    Linear,

    /// <summary>
    /// Exponential backoff with jitter.
    /// </summary>
    ExponentialWithJitter
}
