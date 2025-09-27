using OrdrMate.Core;

namespace OrdrMate.Features.Orders.TableReservation;

public class TableReservationModule : IModule
{
    
    public void Register(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddScoped<TableReservationService>();
    }

}