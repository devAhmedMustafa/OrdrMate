using Microsoft.EntityFrameworkCore;
using OrdrMate.Data;
using OrdrMate.Models;

namespace OrdrMate.Repositories;

public class RestaurantRepo(OrdrMateDbContext c) : IRestaurantRepo
{

    private readonly OrdrMateDbContext _db = c;
    public async Task<Restaurant> CreateRestaurant(Restaurant restaurant)
    {
        _db.Restaurant.Add(restaurant);

        var profile = new RestaurantProfile
        {
            RestaurantId = restaurant.Id,
            Description = "Welcome to our restaurant!",
            LogoUrl = "https://example.com/default-logo.png",
            CoverImageUrl = "https://example.com/default-cover.png"
        };

        _db.RestaurantProfile.Add(profile);

        await _db.SaveChangesAsync();
        return restaurant;
    }

    public async Task<Restaurant?> GetRestaurantById(string id)
    {
        return await _db.Restaurant.FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<bool> HasAccessToRestaurant(string managerId, string restaurantId)
    {
        var restaurant = await _db.Restaurant
            .FirstOrDefaultAsync(r => r.Id == restaurantId);
        if (restaurant == null)
        {
            return false;
        }

        return restaurant.ManagerId == managerId;
    }

    public async Task<Restaurant?> GetRestaurantByManagerId(string managerId)
    {
        return await _db.Restaurant
            .FirstOrDefaultAsync(r => r.ManagerId == managerId);
    }

    public async Task<IEnumerable<Restaurant>> GetAllRestaurants()
    {
        return await _db.Restaurant.ToListAsync();
    }

    public async Task<IEnumerable<Category>> GetRestaurantCategories(string restaurantId)
    {
        var restaurant = await _db.Restaurant
            .Include(r => r.Categories)
            .FirstOrDefaultAsync(r => r.Id == restaurantId);
        if (restaurant == null)
        {
            throw new InvalidOperationException($"Restaurant with id {restaurantId} does not exist.");
        }

        return restaurant.Categories.Select(c => new Category
        {
            Name = c.Name,
            RestaurantId = c.RestaurantId
        }).ToList();
    }

    public async Task<RestaurantProfile?> GetRestaurantProfile(string restaurantId)
    {
        var profile = await _db.RestaurantProfile
            .FirstOrDefaultAsync(p => p.RestaurantId == restaurantId);

        if (profile == null)
        {
            profile = new RestaurantProfile
            {
                RestaurantId = restaurantId,
                Description = "Welcome to our restaurant!",
                LogoUrl = "https://example.com/default-logo.png",
                CoverImageUrl = "https://example.com/default-cover.png"
            };
            _db.RestaurantProfile.Add(profile);
            await _db.SaveChangesAsync();
        }

        return profile;
    }

    public async Task<RestaurantProfile?> UpdateRestaurantProfile(string restaurantId, RestaurantProfile profile)
    {
        var existingProfile = await _db.RestaurantProfile
            .FirstOrDefaultAsync(p => p.RestaurantId == restaurantId);

        if (existingProfile == null)
        {
            throw new InvalidOperationException($"Profile for restaurant {restaurantId} does not exist.");
        }

        existingProfile.Description = profile.Description;
        existingProfile.LogoUrl = profile.LogoUrl;
        existingProfile.CoverImageUrl = profile.CoverImageUrl;

        _db.RestaurantProfile.Update(existingProfile);
        await _db.SaveChangesAsync();

        return existingProfile;
    }
}