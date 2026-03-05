using JG.WorkerKit;
using Xunit;

namespace JG.WorkerKit.Tests;

public class RetryPolicyTests
{
    [Fact]
    public void Exponential_CreatesPolicyWithCorrectValues()
    {
        var policy = RetryPolicy.Exponential(3);
        Assert.Equal(3, policy.MaxRetries);
        Assert.Equal(RetryDelayStrategy.ExponentialWithJitter, policy.DelayStrategy);
        Assert.Equal(TimeSpan.FromSeconds(1), policy.InitialDelay);
        Assert.Equal(TimeSpan.FromMinutes(1), policy.MaxDelay);
    }

    [Fact]
    public void Fixed_CreatesPolicyWithCorrectValues()
    {
        var delay = TimeSpan.FromSeconds(5);
        var policy = RetryPolicy.Fixed(2, delay);
        Assert.Equal(2, policy.MaxRetries);
        Assert.Equal(RetryDelayStrategy.Fixed, policy.DelayStrategy);
        Assert.Equal(delay, policy.InitialDelay);
        Assert.Equal(delay, policy.MaxDelay);
    }

    [Fact]
    public void Linear_CreatesPolicyWithCorrectValues()
    {
        var initialDelay = TimeSpan.FromSeconds(1);
        var increment = TimeSpan.FromSeconds(2);
        var maxDelay = TimeSpan.FromSeconds(10);
        var policy = RetryPolicy.Linear(3, initialDelay, increment, maxDelay);
        Assert.Equal(3, policy.MaxRetries);
        Assert.Equal(RetryDelayStrategy.Linear, policy.DelayStrategy);
        Assert.Equal(initialDelay, policy.InitialDelay);
        Assert.Equal(maxDelay, policy.MaxDelay);
    }
}
