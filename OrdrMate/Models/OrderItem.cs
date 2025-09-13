namespace OrdrMate.Models;

public class OrderItem
{
    public required string OrderId { get; set; }
    public required string ItemId { get; set; }
    public required decimal Price { get; set; }
    public int Quantity { get; set; } = 1;

    public Order? Order { get; set; }
    public Item? Item { get; set; }
}