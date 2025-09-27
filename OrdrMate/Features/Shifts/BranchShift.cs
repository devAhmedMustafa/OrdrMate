namespace OrdrMate.Features.Shifts
{
    public class BranchShift
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public required string BranchId { get; set; }
        public DateTime ShiftStartTime { get; set; } = DateTime.UtcNow;
        public DateTime? ShiftEndTime { get; set; } = null;
        public ShiftStatus Status { get; set; } = ShiftStatus.Started;
    }
}
