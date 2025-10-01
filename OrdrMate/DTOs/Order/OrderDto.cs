namespace OrdrMate.DTOs.Order;

public class OrderDto
{
    public required string OrderId { get; set; }
    public required string StoreName { get; set; }
    public required string CustomerId { get; set; }
    public required string Customer { get; set; }
    public required string CustomerPhone { get; set; }
    public required string OrderType { get; set; }
    public OrderItemDto[]? OrderItems { get; set; }
    public required string PaymentMethod { get; set; }
    public required DateTime OrderDate { get; set; }
    public required string OrderStatus { get; set; }
    public string? PaymentRedirectUrl { get; set; }
    public required decimal TotalAmount { get; set; }
    public required string BranchId { get; set; }
    public bool IsPaid { get; set; } = false;
    public DeliveryDetailsDto? DeliveryDetails { get; set; }

    public int? OrderNumber { get; set; }
}