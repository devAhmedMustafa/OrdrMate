using OrdrMate.Core;

namespace OrdrMate.Features.Storage;

public class StorageModule : IModule
{
    public void Register(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddScoped<IStorageService, StorageService>();
    }
}