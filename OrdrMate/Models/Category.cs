namespace OrdrMate.Models;

public class Category
{
    public required string Name { get; set; }
    public required string PharmacyId { get; set; }
    public Pharmacy? Pharmacy { get; set; }
    public ICollection<Item> Items { get; set; } = [];
}