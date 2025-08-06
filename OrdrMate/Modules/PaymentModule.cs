using OrdrMate.Core;
using OrdrMate.Repositories;
using OrdrMate.Services;

namespace OrdrMate.Modules;

public class PaymentModule : IModule
{
    public void Register(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment env)
    {
        services.AddScoped<IPaymentRepo, PaymentRepo>();
        services.AddScoped<PaymentService, PaymentService>();

        services.AddHttpClient<PaymobService, PaymobService>();
    }
}