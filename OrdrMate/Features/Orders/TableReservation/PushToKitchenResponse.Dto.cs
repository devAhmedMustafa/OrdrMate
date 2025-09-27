using OrdrMate.DTOs.Order;

namespace OrdrMate.Features.Orders.TableReservation;

public record PushToKitchenResponseDto
{
    public required OrderItemDto[] OrderItems { get; set; }
}