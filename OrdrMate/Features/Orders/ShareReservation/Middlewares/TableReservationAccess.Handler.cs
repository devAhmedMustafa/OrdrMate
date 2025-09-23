using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace OrdrMate.Features.Orders.ShareReservation.Middlewares;

public class TableReservationAccessHandler : AuthorizationHandler<TableReservationAccessRequirement, string>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        TableReservationAccessRequirement requirement,
        string reservationId)
    {
        var hasAccess = context.User.HasClaim(c => c.Type == "reservationId" && c.Value == reservationId);

        if (hasAccess)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }

        return Task.CompletedTask;
    }
}