
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
namespace stats;

public static class DIContainer
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services
            .AddSingleton<ICommandBuilder, StatsCommandBuilder>();
        services.AddHttpClient<SprintService>()
        .AddHttpMessageHandler<CacheHandler>()
        .AddHttpMessageHandler<LoggingHandler>();
        services.AddSingleton<ISprintServiceFactory, SprintServiceFactory>();
        services.AddSingleton<ISprintService, SprintService>();
        services.AddSingleton<IStatsCommandHandler, StatsCommandHandler>();
        services.AddSingleton<SprintOptionsBinder, SprintOptionsBinder>();
        services.AddSingleton<FileOptionsBinder, FileOptionsBinder>();
        services.AddSingleton<ISystemHelpers, SystemHelpers>();
    }
}

public static class Container
{
    private static IServiceProvider _serviceProvider;
    private static readonly ServiceCollection services = new ServiceCollection();

    internal static void Flush()
    {
        services.Clear();
        DIContainer.ConfigureServices(services);
        services.AddSingleton<Executor,Executor>();
        _serviceProvider = services.BuildServiceProvider();
    }


    static Container()
    {
        Flush();
    }

    public static T GetService<T>()
    {
        return _serviceProvider.GetService<T>();
    }



    public static void Replace<T> (T service)
    {
        services.Replace(
            new ServiceDescriptor(
                typeof(T),
                service)
                );
        _serviceProvider = services.BuildServiceProvider();
        
    }
}