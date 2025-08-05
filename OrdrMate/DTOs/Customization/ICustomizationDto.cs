namespace OrdrMate.DTOs.Customization;

using OrdrMate.Schemas.CustomizationMetadata;
using OrdrMate.Enums;

public interface ICustomizationDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string RestaurantId { get; set; }
    public CustomizationInputType? InputType { get; set; }
    public ICustomizationMetadata? Metadata { get; }
}