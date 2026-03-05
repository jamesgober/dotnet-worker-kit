using JG.WorkerKit;
using JG.WorkerKit.Internal;
using Microsoft.Extensions.Options;
using Xunit;

namespace JG.WorkerKit.Tests;

public class JobQueuePriorityTests
{
    [Fact]
    public async Task DequeueAsync_PrioritizesCriticalOverNormal()
    {
        var options = new WorkerKitOptions { QueueCapacity = 10 };
        var queue = new JobQueue(new OptionsWrapper<WorkerKitOptions>(options));

        await queue.EnqueueAsync<TestJobHandler, string>("normal", new JobOptions { Priority = JobPriority.Normal });
        await queue.EnqueueAsync<TestJobHandler, string>("critical", new JobOptions { Priority = JobPriority.Critical });

        var item1 = await queue.DequeueAsync();
        var item2 = await queue.DequeueAsync();

        Assert.NotNull(item1);
        Assert.Equal("critical", item1.Value.Data);
        Assert.NotNull(item2);
        Assert.Equal("normal", item2.Value.Data);
    }

    [Fact]
    public async Task DequeueAsync_PrioritizesNormalOverLow()
    {
        var options = new WorkerKitOptions { QueueCapacity = 10 };
        var queue = new JobQueue(new OptionsWrapper<WorkerKitOptions>(options));

        await queue.EnqueueAsync<TestJobHandler, string>("low", new JobOptions { Priority = JobPriority.Low });
        await queue.EnqueueAsync<TestJobHandler, string>("normal", new JobOptions { Priority = JobPriority.Normal });

        var item1 = await queue.DequeueAsync();
        var item2 = await queue.DequeueAsync();

        Assert.NotNull(item1);
        Assert.Equal("normal", item1.Value.Data);
        Assert.NotNull(item2);
        Assert.Equal("low", item2.Value.Data);
    }

    [Fact]
    public async Task EnqueueAsync_WithNullOptions_UsesDefault()
    {
        var options = new WorkerKitOptions { QueueCapacity = 10 };
        var queue = new JobQueue(new OptionsWrapper<WorkerKitOptions>(options));

        await queue.EnqueueAsync<TestJobHandler, string>("test", null);

        var item = await queue.DequeueAsync();

        Assert.NotNull(item);
        Assert.Equal("test", item.Value.Data);
    }
}
