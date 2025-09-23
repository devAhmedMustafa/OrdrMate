namespace OrdrMate.Features.Orders.ShareReservation;

public record GenerateLinkRequest
{
    public required string ReservationId { get; init; }
}