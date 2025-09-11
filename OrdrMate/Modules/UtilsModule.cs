using OrdrMate.Core;
using OrdrMate.Utils;

namespace OrdrMate.Modules;

public class UtilsModule : IModule
{
    public void Register(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment
    )
    {
        services.AddScoped<GeoMaps>();
    }
}