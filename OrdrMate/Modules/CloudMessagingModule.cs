using OrdrMate.Core;
using OrdrMate.Services;

namespace OrdrMate.Modules;

public class CloudMessagingModule : IModule
{
    public void Register(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment env)
    {
        services.AddScoped<CloudMessaging>();
    }
}