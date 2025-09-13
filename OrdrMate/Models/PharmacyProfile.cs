namespace OrdrMate.Models;

public class PharmacyProfile
{
    public required string PharmacyId { get; set; }
    public required string Description { get; set; }
    public required string LogoUrl { get; set; }
    public required string CoverImageUrl { get; set; }

    public Pharmacy? Pharmacy { get; set; }
}