using Hangfire;
using OrdrMate.Features.Orders.Delivery.Dtos;
using OrdrMate.Features.Riders;
using OrdrMate.Features.Riders.GeoLocation;
using OrdrMate.Utils.Exceptions;

namespace OrdrMate.Features.Orders.Delivery;

public class DeliveryDispatcher
{

    private readonly RiderGeoService _riderGeoService;
    private readonly RiderWebSocketHandler _riderWebSocketHandler;
    private readonly DeliveryRepo _deliveryRepo;

    public DeliveryDispatcher(
        RiderGeoService riderGeoService, 
        RiderWebSocketHandler riderWebSocketHandler,
        DeliveryRepo deliveryRepo)
    {
        _riderGeoService = riderGeoService;
        _riderWebSocketHandler = riderWebSocketHandler;
        _deliveryRepo = deliveryRepo;
    }

    public async Task AssignBestRider(string orderId)
    {
        try
        {
            var order = await _deliveryRepo.GetDeliveryByOrderId(orderId);

            if (order == null || order.Order == null)
            {
                throw new NotFoundException($"Order with ID {orderId} not found for delivery assignment.");
            }

            // Search for closest available rider using RiderGeoService
            var riderId = await _riderGeoService.GetClosestAvailableRider(order.PickupLatitude, order.PickupLongitude);

            // Notify the rider about the new delivery assignment
            await _riderWebSocketHandler.NotifyNewDeliveryAssignment(riderId, new DeliveryAssigmentMessage
            {
                PickupLatitude = order.PickupLatitude,
                PickupLongitude = order.PickupLongitude,
                PickupAddress = order.PickupAddress,
                DropoffLatitude = order.DropoffLatitude,
                DropoffLongitude = order.DropoffLongitude,
                DropoffAddress = order.DropoffAddress,
                StoreName = order.Order.Branch?.Restaurant!.Name!,
                TotalAmount = order.Order.TotalAmount
            });

            // Schedule a follow-up if the rider does not respond in time
            BackgroundJob.Schedule<DeliveryDispatcher>(
                dispatcher => dispatcher.AssignBestRider(orderId),
                TimeSpan.FromMinutes(5)
            );
        }
        catch (Exception ex)
        {
            throw new InternalServerException($"Failed to assign rider {ex.Message}");
        }
    }

    public async Task CancelDeliveryAssignment(string orderId)
    {
        try
        {
            var order = await _deliveryRepo.GetDeliveryByOrderId(orderId);

            if (order == null || order.Order == null)
            {
                throw new NotFoundException($"Order with ID {orderId} not found for delivery cancellation.");
            }

            // Clear the background job queue for this order
            BackgroundJob.Delete(orderId);

            // Reassign the delivery to another rider
            await AssignBestRider(orderId);
        }
        catch (Exception ex)
        {
            throw new InternalServerException($"Failed to cancel delivery assignment: {ex.Message}");
        }
    }

    public async Task AcceptAssignment(string orderId, string riderId)
    {
        try
        {
            var order = await _deliveryRepo.GetDeliveryByOrderId(orderId);

            if (order == null || order.Order == null)
            {
                throw new NotFoundException($"Order with ID {orderId} not found for delivery acceptance.");
            }

            // Mark the delivery as accepted by the rider
            order.AssignedRiderId = riderId;
            await _deliveryRepo.UpdateDelivery(order);

            // Clear any pending background jobs for this order
            BackgroundJob.Delete(orderId);
        }
        catch (Exception ex)
        {
            throw new InternalServerException($"Failed to accept delivery assignment: {ex.Message}");
        }
    }

}