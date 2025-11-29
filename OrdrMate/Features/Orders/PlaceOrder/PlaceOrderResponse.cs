namespace OrdrMate.Features.Orders.PlaceOrder;

public class PlaceOrderResponse
{
    public string OrderId { get; set; } = string.Empty;
    public decimal SubtotalAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? RedirectUrl { get; set; }
}