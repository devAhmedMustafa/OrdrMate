namespace OrdrMate.Features.Shifts
{
    public class BranchShift
    {
        public string Id { get; set; }  
        public string BranchId { get; set; }  
        public DateTime? ShiftStartTime { get; set; } 
        public DateTime? ShiftEndTime { get; set; }  
        public ShiftStatus Status { get; set; }  
    }
}
