namespace OrdrMate.DTOs.Store;

public class StoreDTO
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Phone { get; set; }
    public string? LogoUrl { get; set; }
    public string? CoverUrl { get; set; }
    public string? Description { get; set; }
}