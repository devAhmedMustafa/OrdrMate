namespace OrdrMate.DTOs.Customization;

public class CustomizationDto
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string RestaurantId { get; set; }
    public required string InputType { get; set; }
    public required object Metadata { get; set; }
}