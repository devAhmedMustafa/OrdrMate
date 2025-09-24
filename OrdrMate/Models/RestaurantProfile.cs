namespace OrdrMate.Models;

public class RestaurantProfile
{
    public string RestaurantId { get; set; }
    public required string Description { get; set; }
    public required string LogoUrl { get; set; }
    public required string CoverImageUrl { get; set; }
    public string? InstaPayLink { get; set; }
    public Restaurant? Restaurant { get; set; }
}