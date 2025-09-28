namespace OrdrMate.Features.Customization.DTOs;

public class OrderItemsCustomizationResponseDto
{
    public required string OrderId { get; set; }
    public required List<OrderItemCustomizationDto> Items { get; set; }
}

public class OrderItemCustomizationDto
{
    public required string ItemId { get; set; }
    public required string OrderId { get; set; }
    public required object Customization { get; set; }
}
