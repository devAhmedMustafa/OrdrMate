namespace OrdrMate.Features.Orders.Delivery.Dtos;

public class DeliveryAssigmentMessage {
    public float PickupLatitude { get; set; }
    public float PickupLongitude { get; set; }
    public string PickupAddress { get; set; } = string.Empty;
    public required string StoreName { get; set; }
    public float DropoffLatitude { get; set; }
    public float DropoffLongitude { get; set; }
    public string DropoffAddress { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}