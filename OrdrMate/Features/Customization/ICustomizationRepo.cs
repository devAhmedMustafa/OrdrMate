using OrdrMate.Models;

namespace OrdrMate.Repositories;

public interface ICustomizationRepo
{
    Task<IEnumerable<CustomizationCategory>> GetAllCategories();
    Task<IEnumerable<CustomizationCategory>> GetCategoriesByRestaurantId(string restaurantId);
    Task<CustomizationCategory?> GetCategoryById(string id);
    Task<CustomizationCategory?> CreateCategory(CustomizationCategory category);
    Task<ItemCustomization?> AssignCategoryToItem(string itemId, string categoryId);
    Task<IEnumerable<ItemCustomization>> GetItemCustomizations(string itemId);
}