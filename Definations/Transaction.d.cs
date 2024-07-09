using System.Text.Json.Serialization;
using System.Transactions;
using Defination;
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
        public string? DateLink { get; set; } = "";
    }

    public interface ITransactionService : IService<Transaction>
    {
        Task Validations(Transaction transaction);
        Task<API.Transactions.List.Result> List(API.Transactions.List.QueryParams? queryParams);
        Task<API.Transactions.ByDate.Detail> ListByDate(string date);
    }
}

namespace API
{
    namespace Transactions
    {
        namespace ByDate
        {
            public class Data : MongoObject
            {
                [JsonInclude]
                [JsonPropertyName("amount")]
                public string Amount { get; private set; }

                [JsonInclude]
                [JsonPropertyName("description")]
                public string Description { get; private set; } = "";

                [JsonInclude]
                [JsonPropertyName("type")]
                public TransactionType Type { get; private set; }

                [JsonInclude]
                [JsonPropertyName("from_bank")]
                public string? FromBank { get; private set; }

                [JsonInclude]
                [JsonPropertyName("to_bank")]
                public string? ToBank { get; private set; }

                [JsonInclude]
                [JsonPropertyName("category")]
                public string Category { get; private set; }

                public Data(double amount, string description, TransactionType type, string? fromBank, string? toBank, string category, string transactionId)
                {
                    Amount = string.Format("{0:#,##0.##}", amount);
                    Description = description;
                    Type = type;
                    FromBank = fromBank;
                    ToBank = toBank;
                    Category = category;
                    Id = transactionId;
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
                public List<Data> Transactions { get; private set; } = new List<Data>();

                public Detail(GroupAmounts group, List<Data> transactions) : base(group.Debit, group.Credit, group.PartialDebit, group.PartialCredit)
                {
                    Transactions = transactions;
                }
            }
        }

        namespace List
        {
            public class QueryParams
            {
                public string? date { get; set; }
                public string? month { get; set; }
                public string? year { get; set; }
            }

            public class TransactionStage
            {
                [BsonElement("total_count")]
                [JsonPropertyName("total_count")]
                public int TotalCount { get; set; }

                [BsonElement("transactions")]
                [JsonPropertyName("transactions")]
                public List<TransactionDetails> Transactions { get; set; } = new List<TransactionDetails>();
            }

            public class Result : TransactionStage
            {
                [BsonElement("categories")]
                [JsonPropertyName("categories")]
                public CategoryData[] Categories { get; set; } = Array.Empty<CategoryData>();
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

                [BsonElement("date_link")]
                [JsonPropertyName("date_link")]
                public string? DateLink { get; set; } = "";
            }

            public class CategoryData
            {
                private double _amount;

                [BsonElement("category")]
                [JsonPropertyName("category")]
                public string Key { get; set; } = string.Empty;
                
                [BsonElement("amount")]
                [JsonPropertyName("amount")]
                public double Value
                {
                    get { return _amount; }
                    set { _amount = Convert.ToDouble(string.Format("{0:0.00}", value)); }
                }
            } 

            public class CategoryWiseData
            {
                [BsonElement("categories")]
                [JsonPropertyName("categories")]
                public CategoryData[] Categories { get; set; } = Array.Empty<CategoryData>();
            } 
        }
    }
}