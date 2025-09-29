namespace OrdrMate.Features.ItemAvailability;

public class ItemAvailabilityResponse
{
    public required string ItemId { get; set; }
    public required string BranchId { get; set; }
    public bool IsAvailable { get; set; } = true;
    public int Stock { get; set; } = 0;
}