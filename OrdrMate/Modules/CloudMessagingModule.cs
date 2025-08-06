using Google.Apis.Auth.OAuth2;
using OrdrMate.Core;
using OrdrMate.Services;

namespace OrdrMate.Modules;

public class CloudMessagingModule : IModule
{
    public void Register(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment env)
    {
        FirebaseAdmin.FirebaseApp.Create(new FirebaseAdmin.AppOptions()
        {
            Credential = GoogleCredential.FromFile("Keys/firebase-adminsdk.json"),
        });
        
        services.AddScoped<CloudMessaging>();
    }
}