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
    public DateTime Date { get; set; } = DateTime.Now;

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

public interface ITransactionService : IService<Transaction> { }