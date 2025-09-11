using OrdrMate.Core;
using OrdrMate.Repositories;
using OrdrMate.Services;

namespace OrdrMate.Modules;

public class KitchenModule : IModule
{
    public void Register(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment env)
    {
        services.AddScoped<IKitchenRepo, KitchenRepo>();
        services.AddScoped<KitchenService, KitchenService>();
    }
}