using Microsoft.AspNetCore.Authorization;
using OrdrMate.Core;
using OrdrMate.Middlewares;
using OrdrMate.Repositories;
using OrdrMate.Services;

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

        services.Configure<AuthorizationOptions>(options =>
        {
            options.AddPolicy("OrderBranchAccess", policy =>
                policy.Requirements.Add(new OrderBranchAccessRequirement()));
        });

        services.AddScoped<OrderBranchAccessHandler>();
    }
}