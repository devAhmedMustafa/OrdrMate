using MongoDB.Driver;
using OrdrMate.Configs;
using OrdrMate.Features.Customization;

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

    public IMongoCollection<UserCustomization> UserCustomizations =>
    _database.GetCollection<UserCustomization>("UserCustomizations");

    public void EnsureIndexes()
    {
        // CustomizationCategory index
        var existingCategoryIndexes = CustomizationCategories.Indexes.List().ToList();
        bool categoryIndexExists = existingCategoryIndexes
            .Any(i => i["name"] == "RestaurantId_Name");

        if (!categoryIndexExists)
        {
            var customizationCategoryIndexKeys = Builders<CustomizationCategory>.IndexKeys
                .Ascending(c => c.RestaurantId)
                .Ascending(c => c.Name);

            var indexOptions = new CreateIndexOptions 
            { 
                Unique = true, 
                Name = "RestaurantId_Name" // Explicit name to check later
            };

            var indexModel = new CreateIndexModel<CustomizationCategory>(customizationCategoryIndexKeys, indexOptions);
            CustomizationCategories.Indexes.CreateOne(indexModel);
        }

        // UserCustomization index
        var existingUserCustomizationIndexes = UserCustomizations.Indexes.List().ToList();
        bool userCustomizationIndexExists = existingUserCustomizationIndexes
            .Any(i => i["name"] == "ItemId_OrderId");

        if (!userCustomizationIndexExists)
        {
            var userCustomizationIndexKeys = Builders<UserCustomization>.IndexKeys
                .Ascending(c => c.ItemId)
                .Ascending(c => c.OrderId);

            var userCustomizationIndexOptions = new CreateIndexOptions 
            { 
                Unique = true, 
                Name = "ItemId_OrderId" 
            };

            var userCustomizationIndexModel = new CreateIndexModel<UserCustomization>(userCustomizationIndexKeys, userCustomizationIndexOptions);
            UserCustomizations.Indexes.CreateOne(userCustomizationIndexModel);
        }
    }
}