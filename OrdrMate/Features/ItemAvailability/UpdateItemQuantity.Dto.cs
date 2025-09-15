namespace OrdrMate.Features.ItemAvailability;

public class UpdateItemQuantityDto
{
    public string ItemId { get; set; } = string.Empty;
    public string BranchId { get; set; } = string.Empty;
    public int Quantity { get; set; }
}