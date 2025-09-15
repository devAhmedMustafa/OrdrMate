namespace OrdrMate.DTOs.Branch;

public class AddBranchRequestDto
{
    public float Lantitude { get; set; }
    public float Longitude { get; set; }
    public string BranchAddress { get; set; } = string.Empty;
    public string BranchPhoneNumber { get; set; } = string.Empty;
    public required string PharmacyId { get; set; }
}