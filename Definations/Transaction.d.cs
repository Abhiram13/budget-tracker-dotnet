using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Defination;

public enum TransactionType
{
    Debit = 1,
    Credit = 2,
    PartialDebit = 3,
    PartialCredit = 4
}

public class Transaction : MongoObject
{
    [BsonElement("amount")]
    [JsonPropertyName("amount")]
    public double Amount { get; set; } = 0;

    [BsonElement("type")]
    [JsonPropertyName("type")]
    public TransactionType Type { get; set; } = TransactionType.Debit;

    [BsonElement("description")]
    [JsonPropertyName("description")]
    public string Description { get; set; } = "";

    [BsonElement("date")]
    [JsonPropertyName("date")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Unspecified)]
    public DateTime Date { get; set; }

    [BsonElement("due")]
    [JsonPropertyName("due")]
    public bool Due { get; set; }

    [BsonElement("from_bank")]
    [JsonPropertyName("from_bank")]
    public string FromBank { get; set; } = "";

    [BsonElement("to_bank")]
    [JsonPropertyName("to_bank")]
    public string ToBank { get; set; } = "";

    [BsonElement("category_id")]
    [JsonPropertyName("category_id")]
    public string CategoryId { get; set; } = "";
}

public class TransactionDateAmounts
{
    [BsonElement("amount")]
    public double Amount {get; set;}

    [BsonElement("date")]
    public DateOnly Date {get; set;}

    [BsonElement("type")]
    public TransactionType type {get; set;}
}

public class TransactionList
{
    [JsonPropertyName("debit")]
    public double Debit {get; set;}

    [JsonPropertyName("credit")]
    public double Credit {get; set;}

    [JsonPropertyName("date")]    
    public DateTime Date {get; set;}

    [JsonPropertyName("count")]
    public int Count {get; set;}
}

public interface ITransactionService : IService<Transaction> { 
    Task Validations(Transaction transaction);
    Task<List<TransactionList>> ListByDate(string? date = null);
}