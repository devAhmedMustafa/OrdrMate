namespace OrdrMate.Features.TableReservation.DistributionSync;

public class UpdateSyncRequest
{
    public required string ReservationId { get; set; }
    public int? TableNumber { get; set; }
    public DateTime? SeatedTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? ReservationStatus { get; set; }
}