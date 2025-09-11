using OrdrMate.Core;
using OrdrMate.Repositories;
using OrdrMate.Services;

namespace OrdrMate.Modules;

public class UserModule : IModule
{
    public void Register(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment
    )
    {
        services.AddScoped<IUserRepo, UserRepo>();
        services.AddScoped<ManagerService, ManagerService>();
        services.AddScoped<CustomerService, CustomerService>();
    }
}