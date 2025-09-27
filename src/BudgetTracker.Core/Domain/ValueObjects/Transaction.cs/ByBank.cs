using System.Collections.Generic;
using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace BudgetTracker.Core.Domain.ValueObject.Transaction.ByBank;

public class ResultByBank
{
    [BsonElement("bank")]
    [JsonPropertyName("bank")]
    public string Bank { get; set; } = string.Empty;

    [BsonElement("data")]
    [JsonPropertyName("data")]
    public List<ByCategory.CategoryData> BankData { get; set; } = new List<ByCategory.CategoryData>();
}