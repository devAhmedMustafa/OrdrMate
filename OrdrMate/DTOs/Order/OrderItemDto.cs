using System.ComponentModel.DataAnnotations;
using OrdrMate.DTOs.Item;

namespace OrdrMate.DTOs.Order;

public class OrderItemDto
{
    public required string ItemId { get; set; }
    public int Quantity { get; set; } = 1;
    [Required] public decimal Price { get; set; }
    public ItemDto? Item { get; set; } = null;
    public Dictionary<string, string>? Customizations { get; set; } = null;
}