using OrdrMate.Core;
using OrdrMate.Services;

namespace OrdrMate.Modules;

public class AiIntegrationModule : IModule
{
    public void Register(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment env)
    {
        services.AddHttpClient<AiService>();
    }
}