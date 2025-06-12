using OrdrMate.DTOs.Order;
using OrdrMate.Enums;
using OrdrMate.Models;
using OrdrMate.Repositories;

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
}