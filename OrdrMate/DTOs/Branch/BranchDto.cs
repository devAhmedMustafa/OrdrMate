namespace OrdrMate.DTOs.Branch;

public class BranchDto
{
    public string BranchId { get; set; }
    public float Latitude { get; set; }
    public float Longitude { get; set; }
    public string BranchAddress { get; set; }
    public string BranchPhoneNumber { get; set; }
    public string RestaurantId { get; set; }
    public string RestaurantName { get; set; }
    public string BranchManagerId { get; set; }
    public TimeSpan StartWorkingHour { get; set; }
    public TimeSpan EndWorkingHour { get; set; }
    public bool[] WorkingDays { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsOpen { get; set; }
    public bool DeliveryEnabled { get; set; }

}