using OrdrMate.Enums;
using OrdrMate.Schemas.CustomizationMetadata;

namespace OrdrMate.DTOs.Customization;

public class CreateSingleSelectDto : ICustomizationDto
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string RestaurantId { get; set; }
    public CustomizationInputType? InputType { get; set; } = CustomizationInputType.SingleSelect;
    public required SingleSelectMetadata Metadata { get; set; }
    ICustomizationMetadata? ICustomizationDto.Metadata {
        get => Metadata;
    }
}