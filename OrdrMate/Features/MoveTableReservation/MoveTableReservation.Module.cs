using OrdrMate.Core;

namespace OrdrMate.Features.MoveTableReservation;

public class MoveTableReservationModule : IModule
{
    public void Register(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddScoped<MoveTableReservationService>();
    }
}