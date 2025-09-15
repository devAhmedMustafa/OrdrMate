namespace OrdrMate.Models;

public class Takeaway
{
    public required string OrderId { get; set; }
    public required int OrderNumber { get; set; }
    public Order? Order { get; set; }
}