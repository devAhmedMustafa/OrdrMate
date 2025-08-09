using OrdrMate.Core;
using OrdrMate.Repositories;
using OrdrMate.Services;
using OrdrMate.Managers;

namespace OrdrMate.Modules;


public class OrderModule : IModule
{
    public void Register(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment
    )
    {
        services.AddScoped<IOrderRepo, OrderRepo>();
        services.AddScoped<OrderService, OrderService>();
        services.AddScoped<IDeliverRequestRepo, DeliverRequestRepo>();
        services.AddScoped<IPaymentRepo, PaymentRepo>();
        services.AddScoped<PaymentService, PaymentService>();

        services.AddScoped<OrderManager>();

    }
}