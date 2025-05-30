using System.ComponentModel.DataAnnotations;

namespace OrdrMate.DTOs;

public class AddItemDto
{
    [Required] public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; } = 0.0m;
    [Required] public string Category { get; set; } = string.Empty;
    [Required] public string RestaurantId { get; set; } = string.Empty;
}