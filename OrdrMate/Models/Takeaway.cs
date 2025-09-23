namespace OrdrMate.Models;

public class Takeaway
{
    public string Id { get; set; } = string.Empty;
    public int OrderNumber { get; set; } = 0;
    public string OrderId { get; set; } = string.Empty;
    public Order? Order { get; set; }
}