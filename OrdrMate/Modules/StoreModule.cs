using Microsoft.AspNetCore.Authorization;
using OrdrMate.Core;
using OrdrMate.Middlewares;
using OrdrMate.Repositories;
using OrdrMate.Services;

namespace OrdrMate.Modules;

public class StoreModule : IModule
{
    public void Register(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment env)
    {
        services.AddScoped<IStoreRepo, StoreRepo>();
        services.AddScoped<StoreService, StoreService>();

        services.Configure<AuthorizationOptions>(options =>
        {
            options.AddPolicy("CanManageStore", policy =>
                policy.Requirements.Add(new ManageStoreRequirement()));
        });

        services.AddScoped<IAuthorizationHandler, ManageStoreHandler>();

    }
}