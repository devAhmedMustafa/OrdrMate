using OrdrMate.Core;

namespace OrdrMate.Features.BranchAttendance;

public class BranchAttendanceModule : IModule
{
    public void Register(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment
        )
    {
        services.AddScoped<BranchAttendanceService, BranchAttendanceService>();
        services.AddScoped<BranchAuthCode, BranchAuthCode>();
    }
}