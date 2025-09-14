namespace OrdrMate.DTOs.Branch;

public class CreateBranchDto
{
    public required string BranchId { get; set; }
    public required float Latitude { get; set; }
    public required float Longitude { get; set; }
    public required string BranchAddress { get; set; }
    public required string BranchPhoneNumber { get; set; }
    public required string PharmacyId { get; set; }
    public required string PharmacyName { get; set; }
    public string? BranchManagerId { get; set; }
    public TimeSpan StartWorkingHour { get; set; }
    public TimeSpan EndWorkingHour { get; set; }
    public bool[]? WorkingDays { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsOpen { get; set; }
}