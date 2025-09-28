using Microsoft.EntityFrameworkCore.Metadata;
using OrdrMate.Repositories;
using OrdrMate.Services;
using OrdrMate.Utils.Exceptions;

namespace OrdrMate.Features.Orders.Update;

public class UpdateOrderService
{
    private readonly IOrderRepo _orderRepo;
    private readonly ITableRepo _tableRepo;
    private readonly PaymentService _paymentService;

    public UpdateOrderService(IOrderRepo orderRepo, ITableRepo tableRepo, PaymentService paymentService)
    {
        _orderRepo = orderRepo;
        _tableRepo = tableRepo;
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

    public async Task UpdateReservationPayment(string reservationId, PaymentUpdateDto paymentUpdateDto)
    {
        try
        {
            var orders = await _tableRepo.GetTableOrdersByReservationId(reservationId);
            if (orders == null)
            {
                throw new Exception("Orders not found");
            }

            foreach (var order in orders)
            {
                if (order.IsPaid)
                {
                    continue;
                }

                if (order.Payment == null)
                {
                    throw new Exception("Order does not have an associated payment");
                }

                await _paymentService.UpdatePaymentProvider(order.Payment.Id, paymentUpdateDto.PaymentProvider);
            }
        }
        catch (Exception ex)
        {
            throw new InternalServerException($"Failed to update reservation payment: {ex.Message}");
        }
    }
}