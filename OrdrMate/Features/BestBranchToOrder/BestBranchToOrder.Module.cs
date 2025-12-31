using OrdrMate.Core;

namespace OrdrMate.Features.BestBranchToOrder;

public class BestBranchToOrderModule : IModule
{
    public void Register(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment
    )
    {
        services.AddScoped<BestBranchToOrderService>();
    }
}