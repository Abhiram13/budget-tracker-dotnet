using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace BudgetTracker.Core.Domain.ValueObject.Transaction.List;

public class QueryParams
{
    public string? Month { get; set; }
    public string? Year { get; set; }
    public string? Type { get; set; }
    public string? SortOrder { get; set; }
}

public class Result
{
    [BsonElement("total_count")]
    [JsonPropertyName("total_count")]
    public int TotalCount { get; set; }

    [BsonElement("transactions")]
    [JsonPropertyName("transactions")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<TransactionDetails>? Transactions { get; set; } = null;

    [BsonElement("categories")]
    [JsonPropertyName("categories")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<CategoryData>? Categories { get; set; } = null;

    [BsonElement("banks")]
    [JsonPropertyName("banks")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<BankData>? Banks { get; set; } = null;
}

public class TransactionDetails
{
    private double _debit;
    private double _credit;

    [BsonElement("debit")]
    [JsonPropertyName("debit")]
    public double? Debit
    {
        get { return _debit; }
        set { _debit = Convert.ToDouble(string.Format("{0:0.00}", value)); }
    }

    [BsonElement("credit")]
    [JsonPropertyName("credit")]
    public double? Credit
    {
        get { return _credit; }
        set { _credit = Convert.ToDouble(string.Format("{0:0.00}", value)); }
    }

    [BsonElement("date")]
    [JsonPropertyName("date")]
    public string Date { get; set; } = "";

    [BsonElement("count")]
    [JsonPropertyName("count")]
    public int Count { get; set; }
}

public class CategoryData
{
    private double _amount;

    [BsonElement("id")]
    [JsonPropertyName("id")]
    public string CategoryId { get; set; } = string.Empty;

    [BsonElement("category")]
    [JsonPropertyName("category")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("amount")]
    [JsonPropertyName("amount")]
    public double Amount
    {
        get { return _amount; }
        set { _amount = Convert.ToDouble(string.Format("{0:0.00}", value)); }
    }
}

public class BankData
{
    private double _amount;

    [BsonElement("id")]
    [JsonPropertyName("id")]
    public string BankId { get; set; } = string.Empty;

    [BsonElement("name")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("amount")]
    [JsonPropertyName("amount")]
    public double Amount
    {
        get { return _amount; }
        set { _amount = Convert.ToDouble(string.Format("{0:0.00}", value)); }
    }
}

public class TransactionCount
{
    [BsonElement("count")]
    [JsonPropertyName("count")]
    public int Count { get; set; }
}