using OrdrMate.Enums;
using OrdrMate.Features.Customization;
using OrdrMate.Features.ItemAvailability;
using OrdrMate.Features.Orders.Delivery.Dtos;
using OrdrMate.Features.Orders.PlaceOrder;
using OrdrMate.Features.Orders.Tax;
using OrdrMate.Features.Payment;
using OrdrMate.Models;
using OrdrMate.Repositories;
using OrdrMate.Utils.Exceptions;

namespace OrdrMate.Features.Orders.Delivery;

public class PlaceDeliveryOrderService : PlaceOrderService<PlaceDeliveryRequest>
{
    private readonly BranchRepo _branchRepo;
    private readonly DeliveryRepo _deliveryRepo;

    public PlaceDeliveryOrderService(
        PaymentServiceFactory paymentServiceFactory,
        ItemAvailabilityService itemAvailabilityService,
        UserCustomizationService userCustomizationService,
        OrderTaxService orderTaxService,
        IOrderRepo orderRepo,
        BranchRepo branchRepo,
        DeliveryRepo deliveryRepo
        )
    : base(paymentServiceFactory, itemAvailabilityService, userCustomizationService, orderTaxService, orderRepo)
    {
        _branchRepo = branchRepo;
        _deliveryRepo = deliveryRepo;
    }

    protected override async Task<decimal> CalculateTotalAmount(PlaceOrderRequest placeOrderRequest, decimal subtotal)
    {
        try
        {
            var taxAmount = await _orderTaxService.GetOrderTax(placeOrderRequest.StoreId);
            if (taxAmount < 0)
            {
                throw new InternalServerException("Failed to retrieve tax amount for delivery order.");
            }
            
            return subtotal + subtotal * taxAmount;
        }
        catch (Exception ex)
        {
            throw new InternalServerException($"Error calculating total amount for delivery order: {ex.Message}");
        }
    }

    protected override Task<Order> SaveOrderRecord(PlaceOrderRequest placeOrderRequest, decimal subtotal, decimal totalAmount)
    {
        try
        {
            var deliveryRequest = placeOrderRequest as PlaceDeliveryRequest;
            if (deliveryRequest == null)
            {
                throw new InternalServerException("Invalid delivery order request.");
            }

            var order = new Order
            {
                CustomerId = placeOrderRequest.CustomerId,
                BranchId = placeOrderRequest.StoreId,
                SubtotalAmount = subtotal,
                TotalAmount = totalAmount,
                OrderType = OrderType.Delivery,
                Status = OrderStatus.Pending,
            };

            var branch = _branchRepo.GetBranchById(placeOrderRequest.StoreId).Result;
            if (branch == null)
            {
                throw new NotFoundException("Branch not found for delivery order.");
            }

            var delivery = new Delivery
            {
                OrderId = order.Id,
                PickupLatitude = branch.Lantitude,
                PickupLongitude = branch.Longitude,
                PickupAddress = branch.Address,
                DropoffLatitude = deliveryRequest.Latitude,
                DropoffLongitude = deliveryRequest.Longitude,
                DropoffAddress = deliveryRequest.DeliveryAddress,
                RecipientName = "",
                RecipientPhone = "",
            };

            var createdOrder = _orderRepo.CreateOrder(order);
            var createdDelivery = _deliveryRepo.CreateDeliveryOrder(delivery);

            return createdOrder;
        }
        catch (Exception ex)
        {
            throw new InternalServerException($"Error saving delivery order record: {ex.Message}");
        }
    }
}