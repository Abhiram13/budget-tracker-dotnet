using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Defination;

public class Category : MongoObject
{
    [BsonElement("name")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
}

public interface ICategoryService : IService<Category> { };