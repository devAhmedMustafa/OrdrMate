namespace OrdrMate.DTOs.Item;

public class UpdateItemDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public decimal? Price { get; set; }
    public string? Category { get; set; }
    public int? Priority { get; set; }
    public string? Tags { get; set; }
    public string? Brand { get; set; }
}