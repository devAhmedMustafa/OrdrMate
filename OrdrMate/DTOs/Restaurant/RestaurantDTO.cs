namespace OrdrMate.DTOs.Restaurant;

public class RestaurantDTO
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string? LogoUrl { get; set; }
    public string? CoverUrl { get; set; }
    public string? Description { get; set; }
}