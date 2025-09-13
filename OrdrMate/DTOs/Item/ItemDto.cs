using System.ComponentModel.DataAnnotations;

namespace OrdrMate.DTOs.Item;

public class ItemDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal Price { get; set; } = 0.0m;
    public required decimal PreparationTime { get; set; } = 0.0m;
    public required string Category { get; set; } = string.Empty;
    [Required] public required string KitchenName { get; set; }
    public string? KitchenId { get; set; }
    public  int Priority { get; set; } = 0;
    public string Tags { get; set; } = string.Empty;
    public bool? IsAvailable { get; set; } = true;
}