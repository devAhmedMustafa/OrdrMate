namespace OrdrMate.DTOs.Branch;

public class AddBranchRequestDto
{
    public required float Latitude { get; set; }
    public required float Longitude { get; set; }
    public string BranchAddress { get; set; } = string.Empty;
    public string BranchPhoneNumber { get; set; } = string.Empty;
    public required string StoreId { get; set; }
}