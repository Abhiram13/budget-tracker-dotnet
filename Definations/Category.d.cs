using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BudgetTracker.Defination
{
    public class Category : MongoObject
    {
        [BsonElement("name")]
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";
    }
}

namespace BudgetTracker.Injectors
{
    public interface ICategoryService : IMongoService<BudgetTracker.Defination.Category> { };
}