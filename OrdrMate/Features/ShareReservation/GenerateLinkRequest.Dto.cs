namespace OrdrMate.Features.ShareReservation;

public record GenerateLinkRequest
{
    public required string ReservationId { get; init; }
}