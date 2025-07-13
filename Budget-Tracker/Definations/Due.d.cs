using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using BudgetTracker.API.Dues;
using BudgetTracker.Defination;
using MongoDB.Bson.Serialization.Attributes;

namespace BudgetTracker.Defination
{
    public enum DueStatus
    {
        Active = 1,
        Ended = 2,
        Hold = 3,
        InProgress = 4,
        InActive = 5
    }

    public class Due : MongoObject
    {
        /// <summary>
        /// To Whom the due amount should be credited
        /// </summary>
        [BsonElement("payee"), JsonPropertyName("payee"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string Payee { get; init; } = string.Empty;

        [Required, BsonElement("principle_amount"), JsonPropertyName("principle_amount")]
        public double PrincipleAmount { get; init; }

        /// <summary>
        /// Generic description of what the due amount for
        /// </summary>
        [Required, BsonElement("description"), JsonPropertyName("description")]
        public string Description { get; init; } = string.Empty;

        [Required, BsonElement("date"), JsonPropertyName("date")]
        public string Date { get; init; } = string.Empty;

        [Required, BsonElement("comment"), JsonPropertyName("comment")]
        public string Comment { get; init; } = string.Empty;

        /// <summary>
        /// Total amount of due owed to the payee
        /// </summary>
        [BsonIgnoreIfDefault, BsonElement("current_principle"), JsonPropertyName("current_principle"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? CurrentPrinciple { get; set; }

        [Required, BsonElement("status"), JsonPropertyName("status")]
        public DueStatus Status { get; set; } = DueStatus.Active;
    }
}

namespace BudgetTracker.API.Dues
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
        public TransactionType Type { get; set; }
    }

    public class DueTransactions
    {
        // [BsonElement("description")]
        // [JsonPropertyName("description")]
        // public string Description { get; set; } = string.Empty;

        // [BsonElement("principle_amount")]
        // [JsonPropertyName("principle_amount")]
        // public double PrincipleAmount { get; set; }

        [BsonElement("current_principle")]
        [JsonPropertyName("current_principle")]
        public double CurrentPrinciple { get; set; }

        // [BsonElement("status")]
        // [JsonPropertyName("status")]
        // public DueStatus? Status { get; set; }

        [BsonElement("transactions")]
        [JsonPropertyName("transactions")]
        public List<Transactions> Transactions { get; set; } = new List<Transactions>();
    }
}

namespace BudgetTracker.Interface
{
    public interface IDues : IMongoService<Due>
    {
        public Task InsertOneAsync(Due body);
        public Task<DueTransactions> GetDueTransactionsAsync(string dueId);        
    }
}