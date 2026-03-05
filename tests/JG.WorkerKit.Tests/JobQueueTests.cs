using JG.WorkerKit;
using JG.WorkerKit.Internal;
using Microsoft.Extensions.Options;
using Xunit;

namespace JG.WorkerKit.Tests;

public class JobQueueTests
{
    [Fact]
    public async Task EnqueueAsync_WithValidData_AddsJobToQueue()
    {
        var options = new WorkerKitOptions { QueueCapacity = 10 };
        var queue = new JobQueue(new OptionsWrapper<WorkerKitOptions>(options));

        await queue.EnqueueAsync<TestJobHandler, string>("test data");

        var item = await queue.DequeueAsync();
        Assert.NotNull(item);
        Assert.Equal("test data", item.Value.Data);
    }

    [Fact]
    public async Task Complete_StopsDequeueOperations()
    {
        var options = new WorkerKitOptions { QueueCapacity = 10 };
        var queue = new JobQueue(new OptionsWrapper<WorkerKitOptions>(options));

        queue.Complete();

        var item = await queue.DequeueAsync();
        Assert.Null(item);
    }
}
