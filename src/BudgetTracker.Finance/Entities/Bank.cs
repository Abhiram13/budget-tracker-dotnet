using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace BudgetTracker.Core.Domain.Entities;

public class Bank : MongoObject
{
    [BsonElement("name")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
}