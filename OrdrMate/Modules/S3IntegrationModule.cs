using OrdrMate.Core;

namespace OrdrMate.Modules;

public class S3IntegrationModule : IModule
{
    public void Register(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment env)
    {
        services.AddScoped<S3Service>();
    }
}