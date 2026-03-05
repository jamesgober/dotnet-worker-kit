namespace JG.WorkerKit;

/// <summary>
/// Represents a retry policy.
/// </summary>
public readonly struct RetryPolicy
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RetryPolicy"/> struct.
    /// </summary>
    /// <param name="maxRetries">The maximum number of retries.</param>
    /// <param name="delayStrategy">The delay strategy.</param>
    /// <param name="initialDelay">The initial delay.</param>
    /// <param name="maxDelay">The maximum delay.</param>
    public RetryPolicy(int maxRetries, RetryDelayStrategy delayStrategy, TimeSpan initialDelay, TimeSpan maxDelay)
    {
        MaxRetries = maxRetries;
        DelayStrategy = delayStrategy;
        InitialDelay = initialDelay;
        MaxDelay = maxDelay;
    }

    /// <summary>
    /// Gets the maximum number of retries.
    /// </summary>
    public int MaxRetries { get; }

    /// <summary>
    /// Gets the delay strategy.
    /// </summary>
    public RetryDelayStrategy DelayStrategy { get; }

    /// <summary>
    /// Gets the initial delay.
    /// </summary>
    public TimeSpan InitialDelay { get; }

    /// <summary>
    /// Gets the maximum delay.
    /// </summary>
    public TimeSpan MaxDelay { get; }

    /// <summary>
    /// Creates an exponential retry policy.
    /// </summary>
    /// <param name="maxRetries">The maximum number of retries.</param>
    /// <param name="initialDelay">The initial delay.</param>
    /// <param name="maxDelay">The maximum delay.</param>
    /// <returns>A retry policy.</returns>
    public static RetryPolicy Exponential(int maxRetries, TimeSpan? initialDelay = null, TimeSpan? maxDelay = null)
    {
        return new RetryPolicy(
            maxRetries,
            RetryDelayStrategy.ExponentialWithJitter,
            initialDelay ?? TimeSpan.FromSeconds(1),
            maxDelay ?? TimeSpan.FromMinutes(1));
    }

    /// <summary>
    /// Creates a fixed retry policy.
    /// </summary>
    /// <param name="maxRetries">The maximum number of retries.</param>
    /// <param name="delay">The fixed delay.</param>
    /// <returns>A retry policy.</returns>
    public static RetryPolicy Fixed(int maxRetries, TimeSpan delay)
    {
        return new RetryPolicy(maxRetries, RetryDelayStrategy.Fixed, delay, delay);
    }

    /// <summary>
    /// Creates a linear retry policy.
    /// </summary>
    /// <param name="maxRetries">The maximum number of retries.</param>
    /// <param name="initialDelay">The initial delay.</param>
    /// <param name="increment">The delay increment.</param>
    /// <param name="maxDelay">The maximum delay.</param>
    /// <returns>A retry policy.</returns>
    public static RetryPolicy Linear(int maxRetries, TimeSpan initialDelay, TimeSpan increment, TimeSpan maxDelay)
    {
        return new RetryPolicy(maxRetries, RetryDelayStrategy.Linear, initialDelay, maxDelay);
    }
}
