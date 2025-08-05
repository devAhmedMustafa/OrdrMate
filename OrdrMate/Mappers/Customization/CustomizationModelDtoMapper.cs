using OrdrMate.Enums;
using OrdrMate.DTOs.Customization;
using OrdrMate.Models;
using OrdrMate.Schemas.CustomizationMetadata;

using MongoDB.Bson;

namespace OrdrMate.Mappers.Customization;

public class CustomizationModelDtoMapper
{
    public static CustomizationCategory MapToCustomizationCategory(CreateSingleSelectDto dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        return new CustomizationCategory
        {
            Name = dto.Name,
            RestaurantId = dto.RestaurantId,
            Description = dto.Description,
            InputType = CustomizationInputType.SingleSelect,
            Metadata = dto.Metadata.ToBsonDocument()
        };
    }

    public static CustomizationDto MapToCustomizationDto(CustomizationCategory category)
    {
        if (category == null) throw new ArgumentNullException(nameof(category));

        return category.InputType switch
        {
            CustomizationInputType.SingleSelect => new CustomizationDto
            {
                Name = category.Name,
                Description = category.Description ?? string.Empty,
                RestaurantId = category.RestaurantId,
                InputType = CustomizationInputType.SingleSelect.ToString(),
                Metadata = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<SingleSelectMetadata>(category.Metadata),
            },
            _ => throw new NotSupportedException($"Customization input type '{category.InputType}' is not supported."),
        };
    }
}