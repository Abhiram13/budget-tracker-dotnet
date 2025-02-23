using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BudgetTracker.Defination
{
    public class Bank : MongoObject
    {
        [BsonElement("name")]
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";
    }
}

namespace BudgetTracker.Interface
{
    public interface IBankService : IMongoService<BudgetTracker.Defination.Bank> { }
}