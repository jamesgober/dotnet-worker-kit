namespace JG.WorkerKit;

/// <summary>
/// Context for job execution.
/// </summary>
public readonly struct JobContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JobContext"/> struct.
    /// </summary>
    /// <param name="attemptNumber">The attempt number.</param>
    /// <param name="enqueueTime">The enqueue time.</param>
    /// <param name="correlationId">The correlation ID.</param>
    public JobContext(int attemptNumber, DateTime enqueueTime, Guid correlationId)
    {
        AttemptNumber = attemptNumber;
        EnqueueTime = enqueueTime;
        CorrelationId = correlationId;
    }

    /// <summary>
    /// Gets the attempt number.
    /// </summary>
    public int AttemptNumber { get; }

    /// <summary>
    /// Gets the enqueue time.
    /// </summary>
    public DateTime EnqueueTime { get; }

    /// <summary>
    /// Gets the correlation ID.
    /// </summary>
    public Guid CorrelationId { get; }
}
