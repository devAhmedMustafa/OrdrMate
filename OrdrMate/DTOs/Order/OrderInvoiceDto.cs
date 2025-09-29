namespace OrdrMate.DTOs.Order;

public class OrderInvoiceDto
{
    public required string OrderId { get; set; }
    public required string OrderNumber { get; set; }
    public required string CustomerName { get; set; }
    public required string PharmacyName { get; set; }
    public required string BranchAddress { get; set; }
    public required decimal TotalAmount { get; set; }
    public required string PaymentMethod { get; set; }
    public required string OrderType { get; set; }
    public required DateTime OrderDate { get; set; }
    public required bool IsPaid { get; set; }
    public required List<OrderItemDto> Items { get; set; } = [];
}