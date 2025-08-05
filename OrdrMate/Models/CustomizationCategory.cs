using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using OrdrMate.Enums;

namespace OrdrMate.Models;

public class CustomizationCategory
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("name")]
    public required string Name { get; set; }

    [BsonElement("description")]
    public string? Description { get; set; }

    [BsonElement("inputType")]
    public required CustomizationInputType InputType { get; set; }

    [BsonElement("metadata")]
    public BsonDocument? Metadata { get; set; }
}