namespace JG.WorkerKit.Internal;

/// <summary>
/// Represents a job item in the queue.
/// </summary>
internal readonly record struct JobItem(Type JobType, object Data, JobOptions Options, Guid CorrelationId, DateTime EnqueueTime);
