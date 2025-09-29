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
        services.AddScoped<ItemAvailabilityRepository>();
        services.AddScoped<ItemAvailabilityService>();

        services.AddHostedService<ItemAvailabilityOrchestrator>();
    }
}