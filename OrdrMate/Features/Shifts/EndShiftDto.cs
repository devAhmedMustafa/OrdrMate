namespace OrdrMate.Features.Shifts
{
    public class EndShiftDto
    {
        public required string BranchId { get; set; }
        public required DateTime EndTime { get; set; }
    }
}
