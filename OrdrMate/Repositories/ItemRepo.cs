using Microsoft.EntityFrameworkCore;
using OrdrMate.Data;
using OrdrMate.Models;
using OrdrMate.Repositories;

public class ItemRepo(OrdrMateDbContext context) : IItemRepo
{
    private readonly OrdrMateDbContext _context = context;

    public async Task<Item?> AddItem(Item item)
    {

        // Check if the restaurant exists
        var restaurant = await _context.Restaurant
            .FirstOrDefaultAsync(r => r.Id == item.RestaurantId);
        if (restaurant == null)
        {
            Console.Error.WriteLine($"Restaurant with ID {item.RestaurantId} not found.");
            throw new Exception("Restaurant not found");
        }


        // Create a category if it doesn't exist
        var category = await _context.Category
            .FirstOrDefaultAsync(c => c.Name == item.CategoryName && c.RestaurantId == item.RestaurantId);

        if (category == null)
        {
            try
            {
                category = new Category
                {
                    Name = item.CategoryName,
                    RestaurantId = item.RestaurantId
                };
                // Add the new category to the database
                await _context.Category.AddAsync(category);

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error creating category: {ex.Message}");
                throw new Exception("Failed to create category");
            }
        }
        else
        {
            Console.WriteLine("----------------------------------------");
            Console.WriteLine($"Category {item.CategoryName} already exists for restaurant {restaurant.Name}");
            Console.WriteLine("----------------------------------------");

        }

        await _context.Item.AddAsync(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<Item?> GetItem(string id)
    {
        var item = await _context.Item
            .Include(i => i.Kitchen)
            .FirstOrDefaultAsync(i => i.Id == id);

        return item;
    }

    public async Task<IEnumerable<Item>> GetAllItems()
    {
        return await _context.Item.ToListAsync();
    }

    public async Task<IEnumerable<Item>> GetItemsByRestaurantId(string restaurantId)
    {
        return await _context.Item
            .Where(i => i.RestaurantId == restaurantId)
            .ToListAsync();
    }

    public async Task<Item?> UpdateItem(string id, Item item)
    {
        var existingItem = await _context.Item.FindAsync(id);
        if (existingItem == null)
        {
            throw new Exception("Item not found");
        }

        existingItem.Name = item.Name;
        existingItem.Description = item.Description;
        existingItem.ImageUrl = item.ImageUrl;
        existingItem.Price = item.Price;
        existingItem.CategoryName = item.CategoryName;
        existingItem.PreperationTime = item.PreperationTime;
        existingItem.KitchenId = item.KitchenId;
        existingItem.Priority = item.Priority;
        existingItem.Tags = item.Tags;

        await _context.SaveChangesAsync();
        return existingItem;
    }

    public async Task<Item?> UpdateItem(Item item)
    {
        var entity = _context.Item.Update(item);
        await _context.SaveChangesAsync();
        return entity.Entity;
    }

    public async Task<bool> DeleteItem(string id)
    {
        var item = await _context.Item.FindAsync(id);
        if (item == null)
        {
            return false;
        }

        _context.Item.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Item>> GetAvailableItemsByBranchId(string branchId)
    {
        var branch = await _context.Branch
        .Include(b => b.KitchenPowers)!.ThenInclude(kp => kp.Kitchen)
        .FirstOrDefaultAsync(b => b.Id == branchId);

        if (branch == null) throw new Exception("Branch not found");
        if (branch.KitchenPowers == null) throw new Exception("Branch has no kitchen powers defined");

        var items = await _context.Item
            .Where(i => i.RestaurantId == branch.RestaurantId)
            .ToListAsync();

        var availableItems = items
            .Where(i => branch.KitchenPowers.Any(kp => kp.KitchenId == i.KitchenId && kp.Units > 0));

        return availableItems;
    }
}