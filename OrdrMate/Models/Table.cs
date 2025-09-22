namespace OrdrMate.Models;

public class Table
{
    public int TableNumber { get; set; }
    public int Seats { get; set; }
    public string BranchId { get; set; } = string.Empty;
    public Branch? Branch { get; set; }
    public bool IsAvailable { get; set; } = true;
    public bool IsFrozen { get; set; } = false; 


}