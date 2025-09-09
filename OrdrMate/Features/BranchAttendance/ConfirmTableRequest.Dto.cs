namespace OrdrMate.Features.BranchAttendance;

public class ConfirmTableRequest
{
    public required string BranchId { get; set; }
    public required string OrderId { get; set; }
    public required int TableNumber { get; set; }
    public required string AuthCode { get; set; }
}