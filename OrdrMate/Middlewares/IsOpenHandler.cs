using Microsoft.AspNetCore.Authorization;
using OrdrMate.Data;
using OrdrMate.Repositories;

namespace OrderMate.Middlewares;

public class IsOpenHandler : AuthorizationHandler<IsOpenRequirement>
{

    private readonly IBranchRepo _branchRepo;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public IsOpenHandler(IBranchRepo branchRepo, IHttpContextAccessor httpContextAccessor)
    {
        _branchRepo = branchRepo;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IsOpenRequirement requirement)
    {
        var branchId = _httpContextAccessor.HttpContext?.Request.Query["branchId"].ToString();

        if (string.IsNullOrEmpty(branchId))
        {
            context.Fail();
            return Task.CompletedTask;
        }

        var branch = _branchRepo.GetBranchById(branchId).Result;
        if (branch == null)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        var currentTime = DateTime.Now.TimeOfDay;
        if (currentTime >= branch.StartWorkingHour && currentTime <= branch.EndWorkingHour)
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