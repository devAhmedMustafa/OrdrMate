using Microsoft.AspNetCore.Authorization;
using OrdrMate.Core;
using OrdrMate.Middlewares;
using OrdrMate.Repositories;
using OrdrMate.Services;

namespace OrdrMate.Modules;

public class RestaurantModule : IModule
{
    public void Register(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment env)
    {
        services.AddScoped<IPharmacyRepo, RestaurantRepo>();
        services.AddScoped<RestaurantService, RestaurantService>();

        services.Configure<AuthorizationOptions>(options =>
        {
            options.AddPolicy("CanManageRestaurant", policy =>
                policy.Requirements.Add(new ManagePharmacyRequirement()));
        });

        services.AddScoped<IAuthorizationHandler, ManagePharmacyHandler>();

    }
}