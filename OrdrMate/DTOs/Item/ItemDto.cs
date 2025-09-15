namespace OrdrMate.DTOs.Item;

public class ItemDto
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public string? ImageUrl { get; set; }
    public decimal? Price { get; set; }
    public required string Category { get; set; }
    public string? SubCategory { get; set; }
    public int Priority { get; set; } = 0;
    public string Tags { get; set; } = string.Empty;
    public required string Brand { get; set; }
    public bool? IsAvailable { get; set; } = true;
}