namespace OrdrMate.DTOs.Branch;

public class BranchRequestDto
{
    public required string BranchRequestId { get; set; }
    public float Lantitude { get; set; }
    public float Longitude { get; set; }
    public required string BranchAddress { get; set; }
    public required string BranchPhoneNumber { get; set; }
    public required string StoreName { get; set; }
}