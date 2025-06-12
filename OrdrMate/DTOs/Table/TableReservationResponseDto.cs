namespace OrdrMate.DTOs.Table;

public class TableReservationResponseDto
{
    public required int TableNumber { get; set; }
    public required string CustomerName { get; set; }
    public required DateTime ReservationDate { get; set; }
    public required string ReservationStatus { get; set; }
    public required string OrderId { get; set; }
    public required string OrderStatus { get; set; }
}