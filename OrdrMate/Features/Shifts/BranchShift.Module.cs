using OrdrMate.Core;

namespace OrdrMate.Features.Shifts;

public class BranchShiftModule : IModule
{
    public void Register(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment
    )
    {
        services.AddScoped<BranchShiftService>();
        services.AddScoped<IBranchShiftRepo, BranchShiftRepo>();
    }
}