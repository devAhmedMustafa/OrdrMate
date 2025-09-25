using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using OrdrMate.Repositories;

namespace OrdrMate.Middlewares;

public class OrderBranchAccessHandler : AuthorizationHandler<OrderBranchAccessRequirement, string>
{
    private readonly IOrderRepo _orderRepo;
    private readonly IBranchRepo _branchRepo;

    public OrderBranchAccessHandler(IOrderRepo orderRepo, IBranchRepo branchRepo)
    {
        _orderRepo = orderRepo;
        _branchRepo = branchRepo;
    }
    
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, OrderBranchAccessRequirement requirement, string orderId)
    {
        var order = await _orderRepo.GetOrderById(orderId);
        if (order == null)
        {
            context.Fail();
            return;
        }
        
        var branch = await _branchRepo.GetBranchById(order.BranchId);
        if (branch == null)
        {
            context.Fail();
            return;
        }

        if (context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value == branch.BranchManagerId)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }

        return;
    }
}