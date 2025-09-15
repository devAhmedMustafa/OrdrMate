using Microsoft.AspNetCore.Authorization;
using OrdrMate.Core;
using OrdrMate.Middlewares;
using OrdrMate.Repositories;
using OrdrMate.Services;

namespace OrdrMate.Modules;

public class PharmacyModule : IModule
{
    public void Register(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment env)
    {
        services.AddScoped<IPharmacyRepo, PharmacyRepo>();
        services.AddScoped<PharmacyService, PharmacyService>();

        services.Configure<AuthorizationOptions>(options =>
        {
            options.AddPolicy("CanManagePharmacy", policy =>
                policy.Requirements.Add(new ManagePharmacyRequirement()));
        });

        services.AddScoped<IAuthorizationHandler, ManagePharmacyHandler>();

    }
}