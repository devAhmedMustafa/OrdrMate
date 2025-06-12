namespace OrdrMate.DTOs.Table;

public class TableWaitingDto
{
    public int TableNumber { get; set; }
    public int WaitingCount { get; set; }
    public decimal WaitingTime { get; set; } // in minutes
}