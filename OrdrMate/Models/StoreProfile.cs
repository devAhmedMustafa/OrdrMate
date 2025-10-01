namespace OrdrMate.Models;

public class StoreProfile
{
    public required string StoreId { get; set; }
    public required string Description { get; set; }
    public required string LogoUrl { get; set; }
    public required string CoverImageUrl { get; set; }

    public Store? Store { get; set; }
}