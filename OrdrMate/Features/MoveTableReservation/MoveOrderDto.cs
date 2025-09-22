namespace OrdrMate.Features.MoveTableReservation;

public class MoveOrderDto
{
    public int ToTableNumber { get; set; }
    public string ReservationId { get; set; } = string.Empty;
}