using System.Text.Json;
using OrdrMate.Features.Orders.Delivery.Dtos;
using OrdrMate.Features.Riders.Dtos;
using OrdrMate.Features.Riders.GeoLocation.Dtos;
using OrdrMate.Sockets;

namespace OrdrMate.Features.Riders;

public class RiderWebSocketHandler : BaseSocketHandler
{

    public RiderWebSocketHandler()
    {
        
    }

    public async Task NotifyNewDeliveryAssignment(string riderId, DeliveryAssigmentMessage message)
    {
        var deliveryAssignmentMessage = JsonSerializer.Serialize(message);
        await SendTo(riderId, deliveryAssignmentMessage);
    }

    protected override void MessageListener(string message)
    {
        var parsedMessage = JsonSerializer.Deserialize<RiderWebSocketReceiveMessage>(message);
        if (parsedMessage == null) return;

        switch (parsedMessage.Type)
        {
            case "UpdateGeoLocation":
                var request = JsonSerializer.Deserialize<GeoLocationDto>(parsedMessage.Payload);
                if (request != null)
                {
                    HandleUpdateGeoLocation(request);
                }
                break;
            default:
                // Unknown message type
                break;
        }
    }

    private void HandleUpdateGeoLocation(GeoLocationDto request)
    {
    }

    
}