using OrdrMate.Data;
using OrdrMate.Models;
using MongoDB.Driver;
using Microsoft.EntityFrameworkCore;

namespace OrdrMate.Features.Customization;

public class CustomizationRepo : ICustomizationRepo
{
    private readonly OrdrMateMongoContext _context;
    private readonly OrdrMateDbContext _dbContext;

    public CustomizationRepo(OrdrMateMongoContext context, OrdrMateDbContext dbContext)
    {
        _context = context;
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<CustomizationCategory>> GetAllCategories()
    {
        return await _context.CustomizationCategories.Find(_ => true).ToListAsync();
    }

    public async Task<CustomizationCategory?> GetCategoryById(string id)
    {
        var objectId = new MongoDB.Bson.ObjectId(id);
        return await _context.CustomizationCategories.Find(c => c.Id == objectId).FirstOrDefaultAsync();
    }

    public async Task<CustomizationCategory?> CreateCategory(CustomizationCategory category)
    {
        if (category == null)
        {
            throw new ArgumentNullException(nameof(category));
        }

        await _context.CustomizationCategories.InsertOneAsync(category);
        return category;
    }

    public async Task<ItemCustomization?> AssignCategoryToItem(string itemId, string categoryId)
    {
        ArgumentNullException.ThrowIfNull(itemId, nameof(itemId));
        ArgumentNullException.ThrowIfNull(categoryId, nameof(categoryId));

        var _ = await GetCategoryById(categoryId)
        ?? throw new Exception("Customization category not found");

        var itemCustomization = new ItemCustomization
        {
            ItemId = itemId,
            CategoryId = categoryId
        };

        await _dbContext.ItemCustomization.AddAsync(itemCustomization);
        await _dbContext.SaveChangesAsync();
        return itemCustomization;
    }

    public async Task<IEnumerable<CustomizationCategory>> GetCategoriesByRestaurantId(string restaurantId)
    {
        ArgumentNullException.ThrowIfNull(restaurantId, nameof(restaurantId));
        return await _context.CustomizationCategories
            .Find(c => c.RestaurantId == restaurantId)
            .ToListAsync();
    }

    public async Task<IEnumerable<ItemCustomization>> GetItemCustomizations(string itemId)
    {
        ArgumentNullException.ThrowIfNull(itemId, nameof(itemId));
        return await _dbContext.ItemCustomization
            .Where(ic => ic.ItemId == itemId)
            .ToListAsync();
    }
}