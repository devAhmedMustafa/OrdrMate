namespace OrdrMate.Models;

public class Item
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public required decimal Price { get; set; }
    public int Priority { get; set; } = 0;
    public string Tags { get; set; } = string.Empty;
    public required string CategoryName { get; set; }
    public required string PharmacyId { get; set; }
    public Category? Category { get; set; }
    public Pharmacy? Pharmacy { get; set; }
    public List<ItemCustomization>? Customizations { get; set; }
}