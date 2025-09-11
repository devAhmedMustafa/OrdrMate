using Microsoft.EntityFrameworkCore;
using OrdrMate.Core;
using OrdrMate.Data;

namespace OrdrMate.Modules;

public class DbModule : IModule
{
    public void Register(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment env)
    {
        services.AddDbContext<OrdrMateDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
    }
}