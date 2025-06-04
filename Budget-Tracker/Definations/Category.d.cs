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
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Please provide valid category name.")]
        public string Name { get; set; } = "";
    }

    public class Category2
    {
        [JsonPropertyName("id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public ObjectId Id { get; set; }

        [Required]
        [BsonElement("name")]
        [JsonPropertyName("name")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Please provide valid category name.")]
        public string Name { get; set; } = "";
    }

    public class Category3
    {
        [BsonElement("name")]
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";
    }
}

namespace BudgetTracker.Interface
{
    public interface ICategoryService : IMongoService<Defination.Category> { };
}