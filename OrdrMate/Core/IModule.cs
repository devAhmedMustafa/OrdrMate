namespace OrdrMate.Core;

public interface IModule
{
    void Register(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment
    );

}