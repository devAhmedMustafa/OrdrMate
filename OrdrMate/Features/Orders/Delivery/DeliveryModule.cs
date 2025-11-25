using OrdrMate.Core;

namespace OrdrMate.Features.Orders.Delivery;

public class DeliveryModule : IModule
{
    public void Register(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddScoped<DeliveryRepo>();
        services.AddScoped<DeliveryDispatcher>();
    }
}