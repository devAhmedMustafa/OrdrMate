using MongoDB.Driver;
using OrdrMate.Configs;
using OrdrMate.Models;

namespace OrdrMate.Data;

public class OrdrMateMongoContext
{
    private readonly IMongoDatabase _database;
    public OrdrMateMongoContext(IConfiguration configuration, IMongoClient mongoClient)
    {
        var settings = configuration.GetSection("MongoDb").Get<MongoDbSettings>();
        _database = mongoClient.GetDatabase(settings?.Database);
    }

    public IMongoCollection<CustomizationCategory> CustomizationCategories =>
    _database.GetCollection<CustomizationCategory>("CustomizationCategories");

    public IMongoCollection<ItemCustomization> ItemCustomizations =>
    _database.GetCollection<ItemCustomization>("ItemCustomizations");
}