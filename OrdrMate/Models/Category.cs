namespace OrdrMate.Models;

public class Category
{
    public required string Name { get; set; }
    public required string PharmacyId { get; set; }
    public string? Parent { get; set; }
    public Pharmacy? Pharmacy { get; set; }
    public Category? ParentCategory { get; set; }
    public ICollection<Category> Subcategories { get; set; } = [];
    public ICollection<Item> Items { get; set; } = [];
}