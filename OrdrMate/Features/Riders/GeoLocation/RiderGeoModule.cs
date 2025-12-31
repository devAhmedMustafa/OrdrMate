using OrdrMate.Core;

namespace OrdrMate.Features.Riders.GeoLocation;

public class RiderGeoModule : IModule
{
    public void Register(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddScoped<RiderGeoRepo>();
        services.AddScoped<RiderGeoService>();
    }
}