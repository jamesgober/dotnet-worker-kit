using JG.WorkerKit;
using Xunit;

namespace JG.WorkerKit.Tests;

public class CronExpressionTests
{
    [Fact]
    public void Constructor_InvalidCron_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new CronExpression("invalid"));
    }

    [Fact]
    public void GetNextOccurrence_EveryMinute_ReturnsNextMinute()
    {
        var cron = new CronExpression("* * * * *");
        var after = new DateTime(2023, 1, 1, 12, 0, 0);
        var next = cron.GetNextOccurrence(after);
        Assert.Equal(new DateTime(2023, 1, 1, 12, 1, 0), next);
    }

    [Fact]
    public void GetNextOccurrence_SpecificMinute_ReturnsCorrectTime()
    {
        var cron = new CronExpression("15 * * * *");
        var after = new DateTime(2023, 1, 1, 12, 10, 0);
        var next = cron.GetNextOccurrence(after);
        Assert.Equal(new DateTime(2023, 1, 1, 12, 15, 0), next);
    }

    [Fact]
    public void GetNextOccurrence_CrossHour_ReturnsNextHour()
    {
        var cron = new CronExpression("0 * * * *");
        var after = new DateTime(2023, 1, 1, 12, 30, 0);
        var next = cron.GetNextOccurrence(after);
        Assert.Equal(new DateTime(2023, 1, 1, 13, 0, 0), next);
    }
}
