using OrdrMate.Features.Orders.PlaceOrder;

namespace OrdrMate.Features.Orders.Delivery.Dtos;

public class PlaceDeliveryRequest : PlaceOrderRequest
{
    public string DeliveryAddress { get; set; } = string.Empty;
    public float Latitude { get; set; }
    public float Longitude { get; set; }
    
}