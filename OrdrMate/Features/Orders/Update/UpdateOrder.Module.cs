using OrdrMate.Core;

namespace OrdrMate.Features.Orders.Update;

public class UpdateOrderModule : IModule
{
    public void Register(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment
    )
    {
        services.AddScoped<UpdateOrderService>();
    }
}