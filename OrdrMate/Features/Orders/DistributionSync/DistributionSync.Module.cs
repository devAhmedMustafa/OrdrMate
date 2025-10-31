using OrdrMate.Core;

namespace OrdrMate.Features.Orders.DistributionSync;

public class DistributionSyncModule : IModule
{
    public void Register(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddScoped<SyncService>();
    }
}