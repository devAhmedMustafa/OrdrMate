using OrdrMate.Schemas.CustomizationMetadata;

namespace OrdrMate.DTOs.Customization;

public class CreateSingleSelectDto : ICustomizationDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string InputType { get; set; } = string.Empty;
    public required SingleChoiceMetadata Metadata { get; set; }
    ICustomizationMetadata? ICustomizationDto.Metadata {
        get => Metadata;
        set
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value), "Metadata cannot be null.");
            Metadata = (SingleChoiceMetadata)value;
        }
    }
}