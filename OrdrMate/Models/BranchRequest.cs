namespace OrdrMate.Models;

public class BranchRequest
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public float Latitude { get; set; }
    public float Longitude { get; set; }
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public required string StoreId { get; set; }
    public Store? Store { get; set; }
}