namespace OrdrMate.Models;

public class Payment
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public required string OrderId { get; set; }
    public required decimal Amount { get; set; }
    public required string PaymentMethod { get; set; }
    public required string Provider { get; set; }
    public required string TransactionId { get; set; }
    public DateTime PaidAt { get; set; } = DateTime.MaxValue;
    public string ExternalRef { get; set; } = string.Empty;
    public Order? Order { get; set; }
}