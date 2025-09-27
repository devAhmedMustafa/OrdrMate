namespace OrdrMate.DTOs.Branch;

public class BranchInfoDetailedResponse
{
    public required string BranchId { get; set; }
    public required string Address { get; set; }
    public required string Phone { get; set; }
    public required string RestaurantName { get; set; }
    public string? LogoUrl { get; set; }
}