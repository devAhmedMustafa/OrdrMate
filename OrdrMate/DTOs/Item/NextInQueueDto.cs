namespace OrdrMate.DTOs.Item;

public class NextInQueueDto
{
    public required string DequeudItemId { get; set; }
    public string? NextItemId { get; set; }
    public required string KitchenName { get; set; }
    public required int KitchenUnit { get; set; }
}