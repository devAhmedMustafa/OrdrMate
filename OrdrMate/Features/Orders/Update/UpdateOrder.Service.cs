using OrdrMate.Repositories;
using OrdrMate.Services;
using OrdrMate.Utils.Exceptions;

namespace OrdrMate.Features.Orders.Update;

public class UpdateOrderService
{
    private readonly IOrderRepo _orderRepo;
    private readonly PaymentService _paymentService;

    public UpdateOrderService(IOrderRepo orderRepo, PaymentService paymentService)
    {
        _orderRepo = orderRepo;
        _paymentService = paymentService;
    }

    public async Task UpdateOrderPayment(string orderId, PaymentUpdateDto paymentUpdateDto)
    {
        try
        {
            var order = await _orderRepo.GetOrderById(orderId);
            if (order == null)
            {
                throw new Exception("Order not found");
            }

            if (order.IsPaid)
            {
                throw new Exception("Cannot update payment provider for a paid order");
            }

            if (order.Payment == null)
            {
                throw new Exception("Order does not have an associated payment");
            }

            await _paymentService.UpdatePaymentProvider(order.Payment.Id, paymentUpdateDto.PaymentProvider);
        }
        catch (Exception ex)
        {
            throw new InternalServerException($"Failed to update order payment: {ex.Message}");
        }
    }
}