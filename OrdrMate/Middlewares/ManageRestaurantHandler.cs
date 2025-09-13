namespace OrdrMate.Middlewares;

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using OrdrMate.Repositories;
using OrdrMate.Enums;

public class ManageRestaurantHandler : AuthorizationHandler<ManageRestaurantRequirement, string>
{
    private readonly IPharmacyRepo _repo;
    public ManageRestaurantHandler(IPharmacyRepo repo)
    {
        _repo = repo;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ManageRestaurantRequirement requirement,
        string restaurantId
        )
    {
        var managerId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // Check manager role
        if (!context.User.IsInRole(UserRole.TopManager.ToString()))
        {
            context.Fail();
            return;
        }

        if (string.IsNullOrEmpty(managerId) || string.IsNullOrEmpty(restaurantId))
        {
            return;
        }

        bool hasAccess = await _repo.HasAccessToRestaurant(managerId, restaurantId);
        if (hasAccess)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
    }
}