namespace OrdrMate.DTOs.Table;

public class TableReservationResponseDto
{
    public required string ReservationId { get; set; }
    public required int TableNumber { get; set; }
    public required string CustomerName { get; set; }
    public required DateTime ReservationDate { get; set; }
    public required string ReservationStatus { get; set; }
}