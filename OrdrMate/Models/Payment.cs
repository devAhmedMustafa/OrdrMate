namespace OrdrMate.Models;

using OrdrMate.Enums;

public class Payment
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string OrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; } = 0.0m;
    public string PaymentMethod { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public DateTime PaidAt { get; set; } = DateTime.MaxValue;
    public string ExternalRef { get; set; } = string.Empty;
    public Order? Order { get; set; }
}