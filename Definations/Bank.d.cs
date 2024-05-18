using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Defination;

public class Bank : MongoObject
{
    [BsonElement("name")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
}

public interface IBankService : IService<Bank> { }