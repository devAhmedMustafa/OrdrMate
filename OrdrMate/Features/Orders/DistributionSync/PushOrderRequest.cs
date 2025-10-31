
using OrdrMate.DTOs.Order;

namespace OrdrMate.Features.Orders.DistributionSync;

public class PushOrderRequest
{
    public required Models.Order Order { get; set; }
    public required OrderItemDto[] OrderItems { get; set; }
    public int? TableNumber { get; set; }
}