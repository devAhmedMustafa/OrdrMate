namespace OrdrMate.Features.Shifts
{
    public class StartShiftDto
    {
        public required string BranchId { get; set; }
        public required DateTime StartTime { get; set; }
    }
}
