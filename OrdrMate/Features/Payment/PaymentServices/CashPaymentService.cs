namespace OrdrMate.Features.Payment.PaymentServices;

public class CashPaymentService : PaymentService
{
    public CashPaymentService(Services.OrderService orderService) : base(orderService)
    {
    }

    public override Task ConfirmPayment(string orderId)
    {
        throw new NotImplementedException();
    }

    public override Task<PaymentIntentDto> HandlePayment(string orderId, decimal totalAmount)
    {
        throw new NotImplementedException();
    }
}