using OrdrMate.Repositories;
using OrdrMate.Services;

namespace OrdrMate.Features.Orders.Tax;

public class OrderTaxService(IRestaurantRepo restaurantRepo, IBranchRepo branchRepo)
{
    private readonly IRestaurantRepo _restaurantRepo = restaurantRepo;
    private readonly IBranchRepo _branchRepo = branchRepo;

    public async Task UpdateOrderTax(string restaurantId, decimal newTax)
    {
        var restaurant = await _restaurantRepo.GetRestaurantById(restaurantId);
        if (restaurant == null)
        {
            throw new KeyNotFoundException($"Restaurant with id {restaurantId} not found.");
        }

        restaurant.OrderTax = newTax;
        await _restaurantRepo.UpdateRestaurantOrderTax(restaurantId, newTax);
    }

    public async Task<decimal> GetOrderTax(string restaurantId)
    {
        var restaurant = await _restaurantRepo.GetRestaurantById(restaurantId);
        if (restaurant == null)
        {
            throw new KeyNotFoundException($"Restaurant with id {restaurantId} not found.");
        }

        return restaurant.OrderTax;
    }

    public async Task<decimal> GetOrderTaxByBranch(string branchId)
    {
        var branch = await _branchRepo.GetBranchById(branchId);
        if (branch == null)
        {
            throw new KeyNotFoundException($"Branch with id {branchId} not found.");
        }

        if (branch.Restaurant == null)
        {
            throw new KeyNotFoundException($"Restaurant for branch id {branchId} not found.");
        }

        return branch.Restaurant.OrderTax;
    }
}