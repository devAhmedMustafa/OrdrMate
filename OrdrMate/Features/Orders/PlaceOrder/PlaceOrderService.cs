using OrdrMate.Features.Customization;
using OrdrMate.Features.ItemAvailability;
using OrdrMate.Features.Orders.Tax;
using OrdrMate.Features.Payment;
using OrdrMate.Models;
using OrdrMate.Repositories;
using OrdrMate.Utils.Exceptions;

namespace OrdrMate.Features.Orders.PlaceOrder;

public abstract class PlaceOrderService<T>
{
    protected readonly PaymentServiceFactory _paymentServiceFactory;
    protected readonly ItemAvailabilityService _itemAvailabilityService;
    protected readonly UserCustomizationService _userCustomizationService;
    protected readonly OrderTaxService _orderTaxService;
    protected readonly IOrderRepo _orderRepo;

    protected PlaceOrderService(
        PaymentServiceFactory paymentServiceFactory,
        ItemAvailabilityService itemAvailabilityService,
        UserCustomizationService userCustomizationService,
        OrderTaxService orderTaxService,
        IOrderRepo orderRepo
        )
    {
        _paymentServiceFactory = paymentServiceFactory;
        _itemAvailabilityService = itemAvailabilityService;
        _userCustomizationService = userCustomizationService;
        _orderTaxService = orderTaxService;
        _orderRepo = orderRepo;
    }

    public async Task<PlaceOrderResponse> PlaceOrder(T placeOrderRequest)
    {
        try
        {
            if (placeOrderRequest is not PlaceOrderRequest placeOrderDto)
            {
                throw new BadRequestException("Invalid order request data.");
            }

            // 1. Validate item availability
            await ValidateItemAvailability(placeOrderDto);
            // 2. Validate user customizations
            await ValidateUserCustomizations(placeOrderDto);
            // 3. Calculate subtotal
            var subtotal = CalculateSubtotal(placeOrderDto);
            // 4. Calculate taxes and total amount
            var totalAmount = await CalculateTotalAmount(subtotal);
            // 5. Save order record
            var order = await SaveOrderRecord(placeOrderDto, subtotal, totalAmount);
            // 6. Process payment
            var paymentIntent = await ProcessPayment(placeOrderDto, totalAmount, order.Id);

            return new PlaceOrderResponse
            {
                OrderId = order.Id,
                RedirectUrl = paymentIntent.RedirectUrl,
                SubtotalAmount = subtotal,
                TotalAmount = totalAmount
            };
        }
        catch(OException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InternalServerException($"Failed to place order: {ex.Message}");
        }
    }

    private async Task ValidateItemAvailability(PlaceOrderRequest placeOrderRequest)
    {
        try
        {
            foreach (var item in placeOrderRequest.OrderItems)
            {
                var isAvailable = await _itemAvailabilityService.IsItemAvailable(item.ItemId, placeOrderRequest.StoreId);
                if (!isAvailable)
                {
                    throw new BadRequestException($"Item with ID {item.ItemId} is not available at branch {placeOrderRequest.StoreId}");
                }
            }
        }
        catch (Exception ex)
        {
            throw new InternalServerException($"Failed to validate item availability: {ex.Message}");
        }
    }

    private async Task ValidateUserCustomizations(PlaceOrderRequest placeOrderRequest)
    {
        try
        {
            foreach (var item in placeOrderRequest.OrderItems)
            {
                if (item.Customizations == null) continue;

                foreach (var customization in item.Customizations)
                {
                    var isValid = await _userCustomizationService.ValidateUserCustomization(item);
                    if (!isValid)
                    {
                        throw new BadRequestException($"Invalid customization for item ID {item.ItemId}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new InternalServerException($"Failed to validate user customizations: {ex.Message}");
        }
    }

    private decimal CalculateSubtotal(PlaceOrderRequest placeOrderRequest)
    {
        decimal subtotal = 0m;

        foreach (var item in placeOrderRequest.OrderItems)
        {
            subtotal += item.Price * item.Quantity;
        }

        return subtotal;
    }

    protected abstract Task<decimal> CalculateTotalAmount(decimal subtotal);
    protected abstract Task<Order> SaveOrderRecord(PlaceOrderRequest placeOrderRequest, decimal subtotal, decimal totalAmount);

    private async Task<PaymentIntentDto> ProcessPayment(PlaceOrderRequest placeOrderRequest, decimal totalAmount, string orderId)
    {
        try
        {
            var paymentDetails = placeOrderRequest.PaymentDetails;
            var paymentService = _paymentServiceFactory.GetPaymentService(paymentDetails.PaymentProvider, paymentDetails.PaymentMethod);

            var paymentResult = await paymentService.HandlePayment(orderId, totalAmount);
            return paymentResult;
        }
        catch (Exception ex)
        {
            throw new InternalServerException($"Failed to process payment: {ex.Message}");
        }
    }
}