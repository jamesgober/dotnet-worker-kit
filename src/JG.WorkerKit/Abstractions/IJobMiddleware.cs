namespace JG.WorkerKit;

/// <summary>
/// Defines middleware for job execution.
/// </summary>
public interface IJobMiddleware
{
    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    /// <param name="context">The job context.</param>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <returns>A task representing the middleware execution.</returns>
    ValueTask InvokeAsync(JobContext context, Func<ValueTask> next);
}
