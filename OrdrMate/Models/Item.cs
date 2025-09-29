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
    public required string Category { get; set; }
    public string? SubCategory { get; set; }
    public required string PharmacyId { get; set; }
    public required string Brand { get; set; }
    public Pharmacy? Pharmacy { get; set; }
}