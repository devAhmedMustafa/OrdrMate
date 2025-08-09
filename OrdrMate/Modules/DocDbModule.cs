using MongoDB.Driver;
using OrdrMate.Configs;
using OrdrMate.Core;
using OrdrMate.Data;

namespace OrdrMate.Modules;

public class DocDbModule : IModule
{
    public void Register(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment env)
    {
        // MongoDB Configuration
        services.Configure<MongoDbSettings>(configuration.GetSection("MongoDb"));
        services.AddSingleton<IMongoClient, MongoClient>(sp =>
        {
            var settings = configuration.GetSection("MongoDb").Get<MongoDbSettings>();
            return new MongoClient(settings?.ConnectionString);
        });

        // MongoDB Context
        services.AddScoped<OrdrMateMongoContext>();
    }
}