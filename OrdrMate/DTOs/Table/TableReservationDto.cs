namespace OrdrMate.DTOs.Table;

using OrdrMate.Models;

public class TableReservationDto
{
    public required int TableNumber { get; set; }
    public IEnumerable<Order>? Orders { get; set; }
    public DateTime? ReservationTime { get; set; } = null;
}