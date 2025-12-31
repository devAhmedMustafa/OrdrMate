using OrdrMate.Core;
using StackExchange.Redis;

namespace OrdrMate.Modules;

public class RedisModule : IModule
{

    public void Register(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment
    )
    {
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var redisConfig = configuration.GetConnectionString("Redis");

            if (string.IsNullOrEmpty(redisConfig))
            {
                throw new InvalidOperationException("Redis connection string is not configured.");
            }

            return ConnectionMultiplexer.Connect(redisConfig);
        });
    }

}