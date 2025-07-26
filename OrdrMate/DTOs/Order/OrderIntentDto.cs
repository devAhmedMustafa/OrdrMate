namespace OrdrMate.DTOs.Order;

public class OrderIntentDto
{
    public required string OrderIntentId { get; set; }
    public required string RedirectUrl { get; set; }
    public string? OrderId { get; set; }
}