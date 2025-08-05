namespace OrdrMate.DTOs.Customization;

using OrdrMate.Schemas.CustomizationMetadata;

public interface ICustomizationDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string InputType { get; set; }
    public ICustomizationMetadata? Metadata { get; set; }
}