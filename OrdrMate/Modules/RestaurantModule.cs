using OrdrMate.Core;
using OrdrMate.Repositories;
using OrdrMate.Services;

namespace OrdrMate.Modules;

public class RestaurantModule : IModule
{
    public void Register(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment env)
    {
        services.AddScoped<IRestaurantRepo, RestaurantRepo>();
        services.AddScoped<RestaurantService, RestaurantService>();
    }
}