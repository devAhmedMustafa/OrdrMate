using OrdrMate.Core;
using Hangfire;
using Hangfire.MemoryStorage;

namespace OrdrMate.Modules;

public class HangfireModule : IModule
{
    public void Register(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment env)
    {
        services.AddHangfire(config =>
        {
            config.UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseMemoryStorage();
        });

        services.AddHangfireServer();

        services.AddScoped<IBackgroundJobClient, BackgroundJobClient>();
    }
}