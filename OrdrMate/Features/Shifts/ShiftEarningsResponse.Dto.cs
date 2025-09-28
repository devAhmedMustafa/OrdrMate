namespace OrdrMate.Features.Shifts;

public class ShiftEarningsResponse
{
    public class Dto
    {
        public string ShiftId { get; set; } = null!;
        public string BranchId { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public decimal TotalEarnings { get; set; }
        public int TotalOrders { get; set; }
    }
}