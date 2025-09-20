namespace OrdrMate.Features.Orders.Tax;

public class UpdateTaxRequest
{
    public required decimal NewTax { get; set; }
    public required string RestaurantId { get; set; }
}