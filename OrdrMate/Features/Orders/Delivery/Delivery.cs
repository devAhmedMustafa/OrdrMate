using OrdrMate.Models;

namespace OrdrMate.Features.Orders.Delivery;

public class Delivery
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public required string OrderId { get; set; }
    public required string RecipientName { get; set; }
    public required string RecipientPhone { get; set; }
    public required float PickupLatitude { get; set; }
    public required float PickupLongitude { get; set; }
    public required string PickupAddress { get; set; } = string.Empty;
    public required float DropoffLatitude { get; set; }
    public required float DropoffLongitude { get; set; }
    public required string DropoffAddress { get; set; } = string.Empty;
    public string? AssignedRiderId { get; set; }
    public Order? Order { get; set; }
}