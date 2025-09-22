using OrdrMate.Core;

namespace OrdrMate.Features.FreezeTable;

public class FreezeTableModule : IModule
{
    public void Register(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment
        )
    {
        services.AddScoped<FreezeTableService>();
        services.AddScoped<FreezeTableRepo>();
    }
}