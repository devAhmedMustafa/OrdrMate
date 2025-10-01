namespace OrdrMate.Models;

public class Branch
{
    public required string Id { get; set; }
    public float Latitude { get; set; }
    public float Longitude { get; set; }
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public TimeSpan StartWorkingHour { get; set; }
    public TimeSpan EndWorkingHour { get; set; }
    public bool[] WorkingDays { get; set; } = [true, true, true, true, true, true, true];
    public required string StoreId { get; set; }
    public required string BranchManagerId { get; set; }
    public User? BranchManager { get; set; }
    public Store? Store { get; set; }
    public ICollection<Order>? Orders { get; set; }
}