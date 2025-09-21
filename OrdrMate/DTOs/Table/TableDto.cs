namespace OrdrMate.DTOs.Table;

public class TableDto
{
    public int TableNumber { get; set; }
    public int Seats { get; set; }
    public string BranchId { get; set; } = string.Empty;
    public int ReservationCount { get; set; }
}