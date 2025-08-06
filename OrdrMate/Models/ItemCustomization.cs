
namespace OrdrMate.Models;

public class ItemCustomization
{
    public required string ItemId { get; set; }

    public required string CategoryId { get; set; }
    public Item? Item { get; set; }
}