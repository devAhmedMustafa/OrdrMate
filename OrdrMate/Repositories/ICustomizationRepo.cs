using OrdrMate.Models;

namespace OrdrMate.Repositories;

public interface ICustomizationRepo
{
    Task<IEnumerable<CustomizationCategory>> GetAllCategories();
    Task<CustomizationCategory?> GetCategoryById(string id);
    Task<CustomizationCategory?> CreateCategory(CustomizationCategory category);
    Task<ItemCustomization?> AssignCategoryToItem(string itemId, string categoryId);
}