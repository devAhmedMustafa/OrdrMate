using OrdrMate.Core;

namespace OrdrMate.Features.UserPassword;

public class UserPasswordModule : IModule
{
    public void Register(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment
        )
    {
        services.AddScoped<UserPasswordService>();
    }
}