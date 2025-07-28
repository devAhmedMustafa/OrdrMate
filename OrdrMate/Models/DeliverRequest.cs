using OrdrMate.Enums;

namespace OrdrMate.Models;

public class DeliverRequest
{
    public required string OrderId { get; set; }
    public DeliverStatus Status { get; set; } = DeliverStatus.Pending;
    public Order? Order { get; set; }
}