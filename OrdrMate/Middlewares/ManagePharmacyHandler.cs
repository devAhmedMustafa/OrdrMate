namespace OrdrMate.Middlewares;

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using OrdrMate.Repositories;
using OrdrMate.Enums;

public class ManagePharmacyHandler : AuthorizationHandler<ManagePharmacyRequirement, string>
{
    private readonly IPharmacyRepo _repo;
    public ManagePharmacyHandler(IPharmacyRepo repo)
    {
        _repo = repo;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ManagePharmacyRequirement requirement,
        string pharmacyId
        )
    {
        var managerId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // Check manager role
        if (!context.User.IsInRole(UserRole.TopManager.ToString()))
        {
            context.Fail();
            return;
        }

        if (string.IsNullOrEmpty(managerId) || string.IsNullOrEmpty(pharmacyId))
        {
            return;
        }

        bool hasAccess = await _repo.HasAccessToPharmacy(managerId, pharmacyId);
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