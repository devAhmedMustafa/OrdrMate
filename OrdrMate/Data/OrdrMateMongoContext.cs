using MongoDB.Driver;
using OrdrMate.Configs;
using OrdrMate.Features.Customization;
using OrdrMate.Models;

namespace OrdrMate.Data;

public class OrdrMateMongoContext
{
    private readonly IMongoDatabase _database;
    public OrdrMateMongoContext(IConfiguration configuration, IMongoClient mongoClient)
    {
        var settings = configuration.GetSection("MongoDb").Get<MongoDbSettings>();
        _database = mongoClient.GetDatabase(settings?.Database);

        if (_database == null)
        {
            throw new InvalidOperationException("MongoDB database is not configured properly.");
        }

        EnsureIndexes();
    }

    public IMongoCollection<CustomizationCategory> CustomizationCategories =>
    _database.GetCollection<CustomizationCategory>("CustomizationCategories");

    public IMongoCollection<ItemCustomization> ItemCustomizations =>
    _database.GetCollection<ItemCustomization>("ItemCustomizations");

    public IMongoCollection<UserCustomization> UserCustomizations =>
    _database.GetCollection<UserCustomization>("UserCustomizations");

    public void EnsureIndexes()
    {
        var indexKeys = Builders<CustomizationCategory>.IndexKeys
            .Ascending(c => c.RestaurantId)
            .Ascending(c => c.Name);

        var indexOptions = new CreateIndexOptions { Unique = true };
        var indexModel = new CreateIndexModel<CustomizationCategory>(indexKeys, indexOptions);
        CustomizationCategories.Indexes.CreateOne(indexModel);
    }
}