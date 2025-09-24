using Microsoft.AspNetCore.Authorization;
using OrdrMate.Core;
using OrdrMate.Features.Orders.ShareReservation.Enums;
using OrdrMate.Features.Orders.ShareReservation.Middlewares;

namespace OrdrMate.Features.Orders.ShareReservation;

public class ShareReservationModule : IModule
{
    public void Register(
        IServiceCollection services,
        IConfiguration _,
        IHostEnvironment __)
    {
        services.Configure<AuthorizationOptions>(options =>
        {
            options.AddPolicy(AuthPolicies.TableReservationAccess, policy =>
                policy.Requirements.Add(new TableReservationAccessRequirement()));
        });
        
        services.AddScoped<IAuthorizationHandler, TableReservationAccessHandler>();

        services.AddSingleton<TableReservationJwtMiddleware>();
        services.AddScoped<ShareReservationService>();
    }
}