using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using JG.WorkerKit;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> to register WorkerKit services.
/// </summary>
public static class WorkerKitServiceCollectionExtensions
{
    /// <summary>
    /// Adds WorkerKit services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">An action to configure the options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddWorkerKit(this IServiceCollection services, Action<WorkerKitOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.Configure(configure);
        services.AddSingleton<IJobQueue, JG.WorkerKit.Internal.JobQueue>();
        services.AddSingleton<IScheduler, JG.WorkerKit.Internal.Scheduler>();
        services.AddHostedService<JG.WorkerKit.Internal.WorkerKitHostedService>();

        return services;
    }

    /// <summary>
    /// Adds a job handler to the service collection.
    /// </summary>
    /// <typeparam name="TJob">The type of the job handler.</typeparam>
    /// <typeparam name="TData">The type of the job data.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddJob<TJob, TData>(this IServiceCollection services)
        where TJob : class, IJobHandler<TData>
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<TJob>();

        return services;
    }

    /// <summary>
    /// Adds a recurring job to the service collection.
    /// </summary>
    /// <typeparam name="TJob">The type of the scheduled task.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="cronExpression">The cron expression.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddRecurringJob<TJob>(this IServiceCollection services, string cronExpression)
        where TJob : class, IScheduledTask
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(cronExpression);

        services.AddScoped<TJob>();
        services.AddSingleton(new ScheduleEntry(typeof(TJob), cronExpression));

        return services;
    }

    /// <summary>
    /// Adds a recurring job to the service collection.
    /// </summary>
    /// <typeparam name="TJob">The type of the scheduled task.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="interval">The interval.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddRecurringJob<TJob>(this IServiceCollection services, TimeSpan interval)
        where TJob : class, IScheduledTask
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<TJob>();
        services.AddSingleton(new ScheduleEntry(typeof(TJob), interval));

        return services;
    }
}
