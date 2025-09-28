using OrdrMate.DTOs.Order;
using OrdrMate.Models;
using OrdrMate.Repositories;
using OrdrMate.Utils.Exceptions;

namespace OrdrMate.Services;

public class PaymentService
{
    private readonly IPaymentRepo _paymentRepo;
    public PaymentService(IPaymentRepo paymentRepo)
    {
        _paymentRepo = paymentRepo;
    }
    public async Task<PaymentDto> AddPayment(OrderIntent orderIntent, string transactionId)
    {

        var payment = new Payment
        {
            OrderId = orderIntent.OrderId!,
            Amount = orderIntent.Amount,
            PaymentMethod = orderIntent.PaymentMethod,
            Provider = orderIntent.PaymentProvider,
            TransactionId = transactionId,
            PaidAt = DateTime.UtcNow,
            ExternalRef = orderIntent.Id,
        };

        await _paymentRepo.CreatePayment(payment);

        var paymentDto = new PaymentDto
        {
            OrderId = payment.OrderId,
            PaidAt = payment.PaidAt,
            TransactionId = payment.TransactionId,
            PaymentMethod = orderIntent.PaymentMethod,
            Amount = orderIntent.Amount,
            Provider = payment.Provider,
        };

        return paymentDto;
    }

    public async Task UpdatePaymentProvider(string paymentId, string newProvider)
    {
        try
        {
            var payment = await _paymentRepo.GetPaymentById(paymentId);
            if (payment == null)
            {
                throw new Exception("Payment not found");
            }

            payment.Provider = newProvider;
            await _paymentRepo.UpdatePayment(payment);
        }
        catch (Exception ex)
        {
            throw new InternalServerException($"Failed to update payment provider: {ex.Message}");
        }
    }
}