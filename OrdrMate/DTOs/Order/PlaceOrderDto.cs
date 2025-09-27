using System.ComponentModel.DataAnnotations;
using OrdrMate.Enums;

namespace OrdrMate.DTOs.Order;

public class PlaceOrderDto
{
    [Required] public required string BranchId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public double Latitude { get; set; } 
    public double Longitude { get; set; } 
    [Required] public required OrderItemDto[] Items { get; set; }
    [Required] public required OrderType OrderType { get; set; }
    [Required] public required string PaymentMethod { get; set; }
    public string? PaymentProvider { get; set; }
    public int? TableNumber { get; set; } = null;
}