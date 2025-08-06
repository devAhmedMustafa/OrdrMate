using OrdrMate.Core;
using OrdrMate.Repositories;
using OrdrMate.Services;

namespace OrdrMate.Modules;

public class BranchModule : IModule
{
    public void Register(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment env)
    {
        services.AddScoped<IBranchRepo, BranchRepo>();
        services.AddScoped<BranchService, BranchService>();
        services.AddScoped<IBranchRequestRepo, BranchRequestRepo>();
    }
    
}