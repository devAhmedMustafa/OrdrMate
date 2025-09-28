namespace OrdrMate.Models;

public class TableReservation
{
    public string ReservationId { get; set; } = Guid.NewGuid().ToString();
    public required string BranchId { get; set; }
    public required string CustomerId { get; set; }
    public required DateTime ReservationTime { get; set; } = DateTime.UtcNow;
    public DateTime SeatedTime { get; set; } = DateTime.MinValue;
    public DateTime EndTime { get; set; } = DateTime.MaxValue;
    public int TableNumber { get; set; }
    public string ReservationStatus { get; set; } = "Queued";

    public Branch? Branch { get; set; }
    public User? Customer { get; set; }
    public ICollection<Order>? Orders { get; set; }
}

    