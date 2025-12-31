using OrdrMate.Services;

namespace OrdrMate.Features.Payment;

public abstract class PaymentService
{
    private readonly OrderService _orderService;

    public PaymentService(OrderService orderService)
    {
        _orderService = orderService;
    }

    public abstract Task<PaymentIntentDto> HandlePayment(string orderId, decimal totalAmount);

    public abstract Task ConfirmPayment(string orderId);
}