namespace OrdrMate.DTOs.Order;

public class PaymentDto
{
    public required string OrderId { get; set; }
    public required string PaymentMethod { get; set; }
    public required decimal Amount { get; set; }
    public required DateTime PaidAt { get; set; }
    public string? TransactionId { get; set; }
    public string? Provider { get; set; }
}