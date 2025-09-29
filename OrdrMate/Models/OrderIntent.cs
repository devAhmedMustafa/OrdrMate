using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using OrdrMate.DTOs.Order;
using OrdrMate.Enums;

namespace OrdrMate.Models;

public class OrderIntent
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public required string CustomerId { get; set; }
    public required string CustomerName { get; set; }
    public required string CustomerPhone { get; set; }
    public required string BranchId { get; set; }
    public required decimal Amount { get; set; }
    public required string PaymentMethod { get; set; }
    public string PaymentProvider { get; set; } = "Cash";
    public OrderType OrderType { get; set; }
    public string? Notes { get; set; } = string.Empty;
    public string? DeliveryDetailsJson { get; set; } = string.Empty;
    [NotMapped]
    public DeliveryDetailsDto? DeliveryDetails 
    {
        get => string.IsNullOrEmpty(DeliveryDetailsJson) ? null : JsonSerializer.Deserialize<DeliveryDetailsDto>(DeliveryDetailsJson);
        set => DeliveryDetailsJson = JsonSerializer.Serialize(value);
    }
    public string OrderItemsJson { get; set; } = string.Empty;
    [NotMapped]
    public List<OrderItemDto> OrderItems
    {
        get => string.IsNullOrEmpty(OrderItemsJson) ? [] : JsonSerializer.Deserialize<List<OrderItemDto>>(OrderItemsJson) ?? new();
        set => OrderItemsJson = JsonSerializer.Serialize(value);
    }
    public string? OrderId { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public User? Customer { get; set; }
    public Branch? Branch { get; set; }
}