using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace BudgetTracker.Core.Domain.Entities;

public class Category : MongoObject
{
    [Required]
    [BsonElement("name")]
    [JsonPropertyName("name")]
    [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Please provide valid category name.")]
    public string Name { get; set; } = "";
}