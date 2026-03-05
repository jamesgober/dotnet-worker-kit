using JG.WorkerKit;
using Xunit;

namespace JG.WorkerKit.Tests;

public class CronExpressionAdvancedTests
{
    [Fact]
    public void GetNextOccurrence_WithStepAndRange_ReturnsCorrectTime()
    {
        var cron = new CronExpression("*/15 * * * *"); // Every 15 minutes
        var after = new DateTime(2023, 1, 1, 12, 7, 0);
        var next = cron.GetNextOccurrence(after);
        Assert.Equal(new DateTime(2023, 1, 1, 12, 15, 0), next);
    }

    [Fact]
    public void GetNextOccurrence_MonthBoundary_HandlesCorrectly()
    {
        var cron = new CronExpression("0 0 * * *"); // Daily at midnight
        var after = new DateTime(2023, 1, 31, 23, 30, 0);
        var next = cron.GetNextOccurrence(after);
        Assert.Equal(new DateTime(2023, 2, 1, 0, 0, 0), next);
    }

    [Fact]
    public void GetNextOccurrence_SpecificDayOfWeek_ReturnsCorrectTime()
    {
        var cron = new CronExpression("0 9 * * 1"); // Mondays at 9am
        var after = new DateTime(2023, 1, 1, 10, 0, 0); // Sunday
        var next = cron.GetNextOccurrence(after);
        Assert.Equal(new DateTime(2023, 1, 2, 9, 0, 0), next); // Next Monday
    }

    [Fact]
    public void Constructor_InvalidFieldCount_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new CronExpression("* * *"));
    }

    [Fact]
    public void Constructor_ValidFiveFields_DoesNotThrow()
    {
        var exception = Record.Exception(() => new CronExpression("0 0 1 1 *"));
        Assert.Null(exception);
    }
}
