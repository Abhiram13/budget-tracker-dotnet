using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using BudgetTracker.Attributes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BudgetTracker.Defination
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

        [Required]
        [EnumDataType(typeof(TransactionType), ErrorMessage = "Invalid transaction type.")]
        [BsonElement("type")]
        [JsonPropertyName("type")]
        public TransactionType Type { get; set; } = TransactionType.Debit;

        [Required]
        [RegularExpression(@"^[A-Za-z0-9,.\s]+$", ErrorMessage = "Please provide valid description.")]
        [BsonElement("description")]
        [JsonPropertyName("description")]
        public string Description { get; set; } = "";

        [Required]
        [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "Please provide valid date.")]
        [MaxDate(ErrorMessage = "Provided date is out of range or invalid.")]
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

        [Required]
        [BsonElement("category_id")]
        [JsonPropertyName("category_id")]
        public string CategoryId { get; set; } = "";
    }    
}

namespace BudgetTracker.Injectors
{
    public interface ITransactionService : IMongoService<Defination.Transaction>
    {
        Task Validations(Defination.Transaction transaction);
        Task<API.Transactions.List.Result> List(API.Transactions.List.QueryParams? queryParams, CancellationToken? cancellationToken = default);
        Task<API.Transactions.ByDate.Data> ListByDate(string date);
    }
}

namespace BudgetTracker.API
{
    namespace Transactions
    {
        namespace ByDate
        {
            public class Transactions
            {                
                [BsonElement("amount")]
                [JsonPropertyName("amount")]
                public double Amount { get; set; }
             
                [BsonElement("description")]
                [JsonPropertyName("description")]
                public string Description { get; set; } = string.Empty;

                [BsonElement("type")]
                [JsonPropertyName("type")]
                public Defination.TransactionType Type { get; set; }

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

            public class Data
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
                public List<Transactions> Transactions { get; set; } = new List<Transactions>();
            }
        }

        namespace List
        {
            public class QueryParams
            {
                public string? Month { get; set; }
                public string? Year { get; set; }
                public string? Type { get; set; }
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
        }
    }
}