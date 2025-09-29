namespace OrdrMate.Models;

public class Delivery
{
    public required string OrderId { get; set; }
    public required string Address { get; set; }
    public required double Latitude { get; set; }
    public required double Longitude { get; set; }
    public DateTime DeliveryTime { get; set; } = DateTime.MaxValue;
    public Order? Order { get; set; }
}