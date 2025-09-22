namespace OrdrMate.DTOs.Restaurant;

public class RestaurantProfileDto
{
    public string RestaurantId { get; set; } = string.Empty;
    public required string Description { get; set; }
    public required string LogoUrl { get; set; }
    public required string CoverImageUrl { get; set; }
    public string? InstaPayLink { get; set; }

}