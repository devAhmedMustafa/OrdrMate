using OrdrMate.Core;

namespace OrdrMate.Features.Orders.Tax;

public class OrderTaxModule : IModule
{
    public void Register(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddScoped<OrderTaxService>();
    }
}