namespace OrdrMate.Services;

using MongoDB.Bson;
using OrdrMate.DTOs.Customization;
using OrdrMate.Models;
using OrdrMate.Repositories;
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
            InputType = Enum.Parse<CustomizationInputType>(category.InputType, true),
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
}