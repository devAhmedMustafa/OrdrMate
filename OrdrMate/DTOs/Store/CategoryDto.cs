namespace OrdrMate.DTOs.Store;

public class CategoryDto
{
    public required string Name { get; set; }
    public string? ParentCategory { get; set; }
}