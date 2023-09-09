
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
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
    }
}