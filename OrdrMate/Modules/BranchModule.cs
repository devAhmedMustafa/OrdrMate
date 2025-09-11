using Microsoft.AspNetCore.Authorization;
using OrdrMate.Core;
using OrdrMate.Middlewares;
using OrdrMate.Repositories;
using OrdrMate.Services;
using OrdrMate.Sockets;

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

        services.AddScoped<BranchSocketHandler>();

        services.Configure<AuthorizationOptions>(options =>
        {
            options.AddPolicy("BranchManager", policy =>
                policy.Requirements.Add(new BranchManagerRequirement()));
        });

        services.AddScoped<IAuthorizationHandler, BranchManagerHandler>();
    }
    
}