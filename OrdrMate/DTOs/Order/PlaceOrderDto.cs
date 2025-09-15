using System.ComponentModel.DataAnnotations;
using OrdrMate.DTOs.Item;
using OrdrMate.Enums;

namespace OrdrMate.DTOs.Order;

public class OrderItemDto
{
    public required string ItemId { get; set; }
    public int Quantity { get; set; } = 1;
    [Required] public decimal Price { get; set; }
    public ItemDto? Item { get; set; } = null;
}

public class PlaceOrderDto
{
    [Required] public required string BranchId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public required OrderItemDto[] Items { get; set; }
    public required OrderType OrderType { get; set; }
    public required string PaymentMethod { get; set; }
    public DeliveryDetailsDto? DeliveryDetails { get; set; } = null;
    public string? Notes { get; set; } = string.Empty;
}

public class DeliveryDetailsDto
{
    public required string Address { get; set; }
    public required double Latitude { get; set; }
    public required double Longitude { get; set; }
}