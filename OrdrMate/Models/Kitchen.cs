namespace OrdrMate.Models;

public class Kitchen
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RestaurantId { get; set; } = string.Empty;

    public Pharmacy? Restaurant { get; set; }
}