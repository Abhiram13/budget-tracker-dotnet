using System.Collections.Generic;
using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using BudgetTracker.Core.Domain.Enums;

namespace BudgetTracker.Core.Domain.ValueObject.Transaction;

public class ByDateTransactions
{
    [BsonElement("debit")]
    [JsonPropertyName("debit")]
    public double Debit { get; set; }

    [BsonElement("credit")]
    [JsonPropertyName("credit")]
    public double Credit { get; set; }

    [BsonElement("partial_debit")]
    [JsonPropertyName("partial_debit")]
    public double PartialDebit { get; set; }

    [BsonElement("partial_credit")]
    [JsonPropertyName("partial_credit")]
    public double PartialCredit { get; set; }

    [BsonElement("transactions")]
    [JsonPropertyName("transactions")]
    public List<TransactionsList> Transactions { get; set; } = new List<TransactionsList>();
}

public class TransactionsList
{
    [BsonElement("amount")]
    [JsonPropertyName("amount")]
    public double Amount { get; set; }

    [BsonElement("description")]
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("type")]
    [JsonPropertyName("type")]
    public TransactionType Type { get; set; }

    [BsonElement("id")]
    [JsonPropertyName("id")]
    public string TransactionId { get; set; } = string.Empty;

    [BsonElement("from_bank")]
    [JsonPropertyName("from_bank")]
    public string? FromBank { get; set; }

    [BsonElement("to_bank")]
    [JsonPropertyName("to_bank")]
    public string? ToBank { get; set; }

    [BsonElement("category")]
    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;
}