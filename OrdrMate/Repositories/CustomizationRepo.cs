using OrdrMate.Data;
using OrdrMate.Models;
using MongoDB.Driver;

namespace OrdrMate.Repositories;

public class CustomizationRepo : ICustomizationRepo
{
    private readonly OrdrMateMongoContext _context;

    public CustomizationRepo(OrdrMateMongoContext context)
    {
        _context = context;
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
                     
        await _context.ItemCustomizations.InsertOneAsync(itemCustomization);
        return itemCustomization;
    }
}