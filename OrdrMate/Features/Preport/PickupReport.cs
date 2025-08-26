namespace OrdrMate.Models
{
    public class PickupReport
    {
        public string Id { get; set; }
        public string OrderId { get; set; }
        public string Status { get; set; }
        public DateTime ReportedTime { get; set; }
        public DateTime? ApprovedTime { get; set; } 
        public string ManagerId { get; set; }
        public string Notes { get; set; }
    }
}
