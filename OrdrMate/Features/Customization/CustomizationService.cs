namespace OrdrMate.Features.Customization;

using MongoDB.Bson;
using OrdrMate.DTOs.Customization;
using OrdrMate.Enums;

public class CustomizationService
{
    private readonly ICustomizationRepo _customizationRepo;
    public CustomizationService(ICustomizationRepo customizationRepo)
    {
        _customizationRepo = customizationRepo;
    }

    public async Task CreateCustomizationCategory(ICustomizationDto category)
    {
        ArgumentNullException.ThrowIfNull(category);

        var customizationCategory = new CustomizationCategory
        {
            Name = category.Name,
            Description = category.Description,
            RestaurantId = category.RestaurantId,
            InputType = (CustomizationInputType)category.InputType!,
            Metadata = category.Metadata.ToBsonDocument()
        };

        await _customizationRepo.CreateCategory(customizationCategory);
    }

    public async Task AssignCategoryToItem(string itemId, string categoryId)
    {
        ArgumentNullException.ThrowIfNull(itemId, nameof(itemId));
        ArgumentNullException.ThrowIfNull(categoryId, nameof(categoryId));

        await _customizationRepo.AssignCategoryToItem(itemId, categoryId);
    }

    public async Task<IEnumerable<CustomizationDto>> GetCustomizationCategories(string restaurantId)
    {
        ArgumentNullException.ThrowIfNull(restaurantId, nameof(restaurantId));

        var categories = await _customizationRepo.GetCategoriesByRestaurantId(restaurantId);
        var customizationDtos = new List<CustomizationDto>();

        foreach (var category in categories)
        {
            var customizationDto = CustomizationModelDtoMapper.MapToCustomizationDto(category);
            customizationDtos.Add(customizationDto);
        }

        return customizationDtos;
    }

    public async Task<IEnumerable<CustomizationDto>> GetItemCustomizations(string itemId)
    {
        ArgumentNullException.ThrowIfNull(itemId, nameof(itemId));
        var itemCustomizations = await _customizationRepo.GetItemCustomizations(itemId);

        if (itemCustomizations == null)
        {
            return [];
        }

        var customizationDtos = new List<CustomizationDto>();

        foreach (var itemCustomization in itemCustomizations)
        {
            var category = await _customizationRepo.GetCategoryById(itemCustomization.CategoryId);
            if (category != null)
            {
                var customizationDto = CustomizationModelDtoMapper.MapToCustomizationDto(category);
                customizationDtos.Add(customizationDto);
            }
        }

        return customizationDtos;
    }
}