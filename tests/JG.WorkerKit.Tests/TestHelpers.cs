using JG.WorkerKit;

namespace JG.WorkerKit.Tests;

public class TestJobHandler : IJobHandler<string>
{
    public ValueTask HandleAsync(string data, JobContext context, CancellationToken cancellationToken = default)
    {
        return ValueTask.CompletedTask;
    }
}
