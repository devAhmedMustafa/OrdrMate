namespace OrdrMate.DTOs.Store;

public class StoreProfileDto
{
    public required string StoreId { get; set; }
    public required string Description { get; set; }
    public required string LogoUrl { get; set; }
    public required string CoverImageUrl { get; set; }
}