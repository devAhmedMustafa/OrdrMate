using OrdrMate.Features.Payment.PaymentServices;

namespace OrdrMate.Features.Payment;

public class PaymentServiceFactory
{
    private readonly CashPaymentService _cashPaymentService;

    public PaymentServiceFactory(CashPaymentService cashPaymentService)
    {
        _cashPaymentService = cashPaymentService;
    }

    public PaymentService GetPaymentService(string provider, string method)
    {
        switch (provider.ToLower())
        {
            case "cash":
                return _cashPaymentService;
            default:
                throw new NotImplementedException($"Payment provider '{provider}' with method '{method}' is not implemented.");
        }
    }
}