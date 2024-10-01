using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BudgetTracker.Defination
{
    public class Category : MongoObject
    {
        [Required]
        [BsonElement("name")]
        [JsonPropertyName("name")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Please provide valid description.")]
        public string Name { get; set; } = "";
    }
}

namespace BudgetTracker.Injectors
{
    public interface ICategoryService : IMongoService<BudgetTracker.Defination.Category> { };
}