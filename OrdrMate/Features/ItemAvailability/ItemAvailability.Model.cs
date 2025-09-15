using OrdrMate.Models;

namespace OrdrMate.Features.ItemAvailability;

public class ItemAvailability
{
    public required string ItemId { get; set; }
    public required string BranchId { get; set; }
    public bool IsAvailable { get; set; } = true;
    public int AvailableQuantity { get; set; } = 0;
    public Item? Item { get; set; }
    public Branch? Branch { get; set; }
}