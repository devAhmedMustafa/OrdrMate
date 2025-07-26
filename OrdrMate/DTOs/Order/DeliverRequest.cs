namespace OrdrMate.DTOs.Order;

using OrdrMate.Enums;

public class DeliverRequestDto
{
    public required string OrderId { get; set; }
    public DeliverStatus Status { get; set; } = DeliverStatus.Pending;
}