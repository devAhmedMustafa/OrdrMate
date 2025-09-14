namespace OrdrMate.DTOs.Item;

public class UpdateItemDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal Price { get; set; } = 0.0m;
    public decimal PreparationTime { get; set; } = 0.0m;
    public string Category { get; set; } = string.Empty;
    public string KitchenId { get; set; } = string.Empty;
    public required int Priority { get; set; } = 0;
    public required string Tags { get; set; } = string.Empty;
}