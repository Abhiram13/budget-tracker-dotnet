using System.Text.Json.Serialization;
using BudgetTracker.Core.Domain.Entities;
using BudgetTracker.Core.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BudgetTracker.Core.Domain.ValueObject.Dues;

public class DueList
{
    [BsonId, BsonRepresentation(BsonType.ObjectId), JsonPropertyName("_id"), BsonElement("_id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name"), BsonElement("name")]
    public string Name { get; set; } = string.Empty;
}

public class DueTransactions
{
    [JsonPropertyName("description"), BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("type"), BsonElement("type")]
    public TransactionType Type { get; set; }

    [JsonPropertyName("transaction_id"), BsonElement("transaction_id")]
    public string TransactionsId { get; set; } = string.Empty;

    [JsonPropertyName("amount"), BsonElement("amount")]
    public double Amount { get; set; }
}

public class DueDetails : Due
{
    [JsonPropertyName("current_principle"), BsonElement("current_principle")]
    public double CurrentPrinciple { get; set; }
}