namespace OrdrMate.Models;

public class Branch
{
    public required string Id { get; set; }
    public float Lantitude { get; set; }
    public float Longitude { get; set; }
    public string Address { get; set; }
    public string Phone { get; set; }
    public TimeSpan StartWorkingHour { get; set; }
    public TimeSpan EndWorkingHour { get; set; }
    public bool[] WorkingDays { get; set; } = new bool[7] { true, true, true, true, true, true, true };
    public required string RestaurantId { get; set; }
    public required string BranchManagerId { get; set; }
    public User? BranchManager { get; set; }
    public Restaurant? Restaurant { get; set; }
    public ICollection<Table>? Tables { get; set; }
    public ICollection<Order>? Orders { get; set; }
    public ICollection<KitchenPower>? KitchenPowers { get; set; }
    public bool DeliveryEnabled { get; set; }
}