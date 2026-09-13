using System.Net;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BudgetTracker.Entities;

public abstract class MongoObject : BaseTimeStampEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonElement("_id")]
    [JsonPropertyName("id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string Id { get; set; } = "";

    [BsonElement("__v")]
    [JsonPropertyName("__v")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public byte? V { get; set; }
}

public abstract class BaseTimeStampEntity
{
    [BsonElement("created_at")]
    public DateTime CreatedAt { get; private set; }
    
    [BsonElement("updated_at")]
    public DateTime UpdatedAt { get; private set; }

    protected void SetModifiedAt()
    {
        DateTime now = DateTime.UtcNow;

        CreatedAt = now;
        UpdatedAt = now;
    }

    protected void SetUpdatedAt()
    {
        DateTime now = DateTime.UtcNow;

        UpdatedAt = now;
    }
}