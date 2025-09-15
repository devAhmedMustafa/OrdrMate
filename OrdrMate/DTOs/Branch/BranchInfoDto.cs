namespace OrdrMate.DTOs.Branch;

public class BranchInfoDto
{
    public required string BranchId { get; set; }
    public required string BranchAddress { get; set; }
    public required string BranchPhoneNumber { get; set; }
    public required string PharmacyId { get; set; }
    public required string PharmacyName { get; set; }
    public required TimeSpan StartWorkingHour { get; set; }
    public required TimeSpan EndWorkingHour { get; set; }
    public required bool[] WorkingDays { get; set; }
    public required bool IsOpen { get; set; }
    public required int OrdersInQueue { get; set; }
}