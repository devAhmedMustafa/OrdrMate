namespace OrdrMate.DTOs.Item;

public class NextInQueueDto
{
    public required string DequeudItemId { get; set; }
    public string? NextItemId { get; set; }
}