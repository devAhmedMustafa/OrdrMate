using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OrdrMate.Features.Customization;

public class UserCustomization
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("userId"), BsonRequired]
    public required string UserId { get; set; }

    [BsonElement("itemId"), BsonRequired]
    public required string ItemId { get; set; }

    [BsonElement("customizationValues")]
    public required BsonDocument CustomizationValues { get; set; }
}