using OrdrMate.Repositories;
using OrdrMate.Services;

namespace OrdrMate.Features.Orders.Tax;

public class OrderTaxService(IRestaurantRepo restaurantRepo)
{
    private readonly IRestaurantRepo _restaurantRepo = restaurantRepo;

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
}