using System.Text.Json.Serialization;
using System.Transactions;
using Defination;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using TransactionsByDate;

namespace Defination
{
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
        public string Date { get; set; } = "";

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

    public class TransactionList<T>
    {
        [JsonPropertyName("debit")]
        public T? Debit { get; set; }

        [JsonPropertyName("credit")]
        public T? Credit { get; set; }

        [JsonPropertyName("date")]
        public string Date { get; set; } = "";

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("date_link")]
        public string? DateLink {get; set;} = "";
    }

    namespace TransactionsList
    {
        public class QueryParams
        {
            public string? date { get; set; }
            public string? month { get; set; }
            public string? year { get; set; }
        }
    }

    public interface ITransactionService : IService<Transaction>
    {
        Task Validations(Transaction transaction);
        Task<List<TransactionList<string>>> List(TransactionsList.QueryParams? queryParams);
        Task<Result> ListByDate(string date);        
    }
}

namespace TransactionsByDate
{
    public class Transactions
    {
        [JsonInclude]
        [JsonPropertyName("amount")]
        public double Amount { get; set; }

        [JsonInclude]
        [JsonPropertyName("description")]
        public string Description { get; set; } = "";

        [JsonInclude]
        [JsonPropertyName("type")]
        public TransactionType Type { get; set; }

        [JsonInclude]
        [JsonPropertyName("from_bank")]
        public string? FromBank { get; set; }

        [JsonInclude]
        [JsonPropertyName("to_bank")]
        public string? ToBank { get; set; }

        [JsonInclude]
        [JsonPropertyName("category")]
        public string Category { get; set; } = "";

        [JsonInclude]
        [JsonPropertyName("id")]
        public string Id { get; set; } = "";
    }

    public class Result
    {
        [JsonPropertyName("transactions")]
        public List<Transactions> Transactions {get; set;} = new List<Transactions>();

        [JsonPropertyName("debit")]
        public double Debit { get; set; }

        [JsonPropertyName("credit")]
        public double Credit { get; set; }

        [JsonPropertyName("partial_debit")]
        public double PartialDebit { get; set; }

        [JsonPropertyName("partial_credit")]
        public double PartialCredit { get; set; }
    } 
}