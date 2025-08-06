using OrdrMate.Core;
using OrdrMate.Repositories;
using OrdrMate.Services;

namespace OrdrMate.Modules;

public class ItemModule : IModule
{
    public void Register(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment env)
    {
        services.AddScoped<IItemRepo, ItemRepo>();
        services.AddScoped<ItemService, ItemService>();

        services.AddScoped<ICustomizationRepo, CustomizationRepo>();
        services.AddScoped<CustomizationService, CustomizationService>();
    }
}