namespace OrdrMate.DTOs.Branch;

public class BranchApprovalDto
{
    public required string BranchId { get; set; }
    public required string BranchAddress { get; set; }
    public required string BranchPhoneNumber { get; set; }
    public required string PharmacyId { get; set; }
    public required string BranchManagerId { get; set; }
    public required string BranchManagerUsername { get; set; }
    public required string BranchManagerPassword { get; set; }
}