using System.Collections.Generic;
using System.Text.Json.Serialization;
using BudgetTracker.Core.Domain.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace BudgetTracker.Core.Domain.ValueObject.Transaction.ByCategory;

public class CategoryTransactions
{
    [BsonElement("amount")]
    [JsonPropertyName("amount")]
    public double Amount { get; set; }

    [BsonElement("type")]
    [JsonPropertyName("type")]
    public TransactionType Type { get; set; }

    [BsonElement("description")]
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}

public class CategoryData
{
    [BsonElement("date")]
    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;

    [BsonElement("transactions")]
    [JsonPropertyName("transactions")]
    public List<CategoryTransactions> Transactions { get; set; } = new List<CategoryTransactions>();
}

public class Result
{
    [BsonElement("category")]
    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [BsonElement("data")]
    [JsonPropertyName("data")]
    public List<CategoryData> CategoryData { get; set; } = new List<CategoryData>();
}