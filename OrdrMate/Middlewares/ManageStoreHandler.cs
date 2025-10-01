namespace OrdrMate.Middlewares;

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using OrdrMate.Repositories;
using OrdrMate.Enums;

public class ManageStoreHandler : AuthorizationHandler<ManageStoreRequirement, string>
{
    private readonly IStoreRepo _repo;
    public ManageStoreHandler(IStoreRepo repo)
    {
        _repo = repo;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ManageStoreRequirement requirement,
        string storeId
        )
    {
        var managerId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // Check manager role
        if (!context.User.IsInRole(UserRole.TopManager.ToString()))
        {
            context.Fail();
            return;
        }

        if (string.IsNullOrEmpty(managerId) || string.IsNullOrEmpty(storeId))
        {
            return;
        }

        bool hasAccess = await _repo.HasAccessToStore(managerId, storeId);
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