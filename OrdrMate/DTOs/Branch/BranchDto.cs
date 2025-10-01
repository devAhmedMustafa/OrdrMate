namespace OrdrMate.DTOs.Branch;

public class BranchDto
{
    public required string BranchId { get; set; }
    public required float Latitude { get; set; }
    public required float Longitude { get; set; }
    public required string BranchAddress { get; set; }
    public required string BranchPhoneNumber { get; set; }
    public string? StoreId { get; set; }
    public required string StoreName { get; set; }
    public string? BranchManagerId { get; set; }
    public TimeSpan StartWorkingHour { get; set; }
    public TimeSpan EndWorkingHour { get; set; }
    public required bool[] WorkingDays { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsOpen { get; set; }
}