using OrdrMate.Core;
using OrdrMate.Repositories;

namespace OrdrMate.Features.Preport;

public class PickupReportModule : IModule
{
    public void Register(IServiceCollection services, IConfiguration configuration, IHostEnvironment env)
    {
        services.AddScoped<IPickupReportRepo, PickupReportRepo>();
        services.AddAutoMapper(typeof(PickupReportProfile));
        services.AddScoped<PickupReportService, PickupReportService>();
    }
}