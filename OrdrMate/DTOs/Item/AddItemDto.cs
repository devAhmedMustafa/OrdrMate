using System.ComponentModel.DataAnnotations;

namespace OrdrMate.DTOs.Item;

public class AddItemDto
{
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public required decimal Price { get; set; }
    public required string Category { get; set; }
    public string? SubCategory { get; set; }
    public int Priority { get; set; } = 0;
    public string Tags { get; set; } = string.Empty;
    public required string Brand { get; set; }
    public required string StoreId { get; set; }
}