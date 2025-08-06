using OrdrMate.Core;
using OrdrMate.Repositories;
using OrdrMate.Services;

namespace OrdrMate.Modules;

public class TableModule : IModule
{
    public void Register(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment env)
    {
        services.AddScoped<ITableRepo, TableRepo>();
        services.AddScoped<TableService, TableService>();
    }
}