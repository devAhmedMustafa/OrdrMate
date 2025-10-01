using System.ComponentModel.DataAnnotations;
using OrdrMate.Enums;

namespace OrdrMate.DTOs.Order;

public class PlaceOrderDto
{
    [Required] public required string StoreId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public required string CustomerName { get; set; }
    public required string CustomerPhone { get; set; }
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