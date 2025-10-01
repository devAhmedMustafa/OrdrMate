using OrdrMate.Models;

namespace OrdrMate.Features.Customization;

public class ItemCustomization
{
    public required string ItemId { get; set; }

    public required string CategoryId { get; set; }
    public Item? Item { get; set; }
}