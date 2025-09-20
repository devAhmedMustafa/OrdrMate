using OrdrMate.Core;

namespace OrdrMate.Features.CashierOrder;

public class CashierOrderModule : IModule
{
    public void Register(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddScoped<CashierOrderService>();
    }
}