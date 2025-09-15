using OrdrMate.Core;
using OrdrMate.Repositories;
using OrdrMate.Services;

namespace OrdrMate.Features.Customization
{
    public class CustomizationModule : IModule
    {
        public void Register(
            IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment env)
        {
            services.AddScoped<ICustomizationRepo, CustomizationRepo>();
            services.AddScoped<CustomizationService, CustomizationService>();
            services.AddScoped<UserCustomizationRepo, UserCustomizationRepo>();
            services.AddScoped<UserCustomizationService, UserCustomizationService>();
        }
    }
}