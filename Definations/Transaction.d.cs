using System.Text.Json.Serialization;
using System.Transactions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

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
        Task<TransactionsByDate.Detail> ListByDate(string date);
        Task ListByMonth();
    }
}

namespace TransactionsByDate
{
    public class List: Defination.MongoObject
    {
        [JsonInclude]
        [BsonElement("amount")]
        [JsonPropertyName("amount")]
        public double Amount { get; private set; }

        [JsonInclude]
        [BsonElement("description")]
        [JsonPropertyName("description")]
        public string Description { get; private set; } = "";

        [JsonInclude]
        [BsonElement("type")]
        [JsonPropertyName("type")]
        public Defination.TransactionType Type { get; private set; }

        public List(double amount, string description, Defination.TransactionType type)
        {
            Amount = amount;
            Description = description;
            Type = type;
        }
    }

    public class GroupAmounts
    {
        [JsonPropertyName("debit")]
        public double Debit { get; private set; }

        [JsonPropertyName("credit")]
        public double Credit { get; private set; }

        [JsonPropertyName("partial_debit")]
        public double PartialDebit { get; private set; }

        [JsonPropertyName("partial_credit")]
        public double PartialCredit { get; private set; }

        public GroupAmounts(double debit, double credit, double partialDebit, double partialCredit)
        {
            Debit = debit;
            Credit = credit;
            PartialCredit = partialCredit;
            PartialDebit = partialDebit;
        }
    }

    public class Detail : GroupAmounts
    {
        [JsonPropertyName("transactions")]
        public List<List> Transactions {get; private set;} = new List<List>();

        public Detail(GroupAmounts group, List<List> transactions) : base(group.Debit, group.Credit, group.PartialDebit, group.PartialCredit) { 
            Transactions = transactions;
        }
    }
}