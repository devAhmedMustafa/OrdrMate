using OrdrMate.Core;

namespace OrdrMate.Features.ItemAvailability;

public class ItemAvailabilityModule : IModule
{
    public void Register(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment
        )
    {
        services.AddScoped<ItemAvailabilityRepository, ItemAvailabilityRepository>();
        services.AddScoped<ItemAvailabilityService, ItemAvailabilityService>();

        services.AddSingleton<ItemAvailabilityOrch, ItemAvailabilityOrch>();
        var provider = services.BuildServiceProvider();
        provider.GetService<ItemAvailabilityOrch>();
    }
}