using OrdrMate.DTOs.Order;

namespace OrdrMate.Features.Orders.PlaceOrder;

public abstract class PlaceOrderRequest
{
    public required string CustomerId { get; init; }
    public required string StoreId { get; init; }
    public required List<OrderItemDto> OrderItems { get; init; }
    public required PaymentDetailsDto PaymentDetails { get; init; }
}

public class PaymentDetailsDto
{
    public required string PaymentMethod { get; init; }
    public required string PaymentProvider { get; init; }
}