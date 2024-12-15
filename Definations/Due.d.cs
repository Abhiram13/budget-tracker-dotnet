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
    }

    public class Due : MongoObject
    {
        [Required, BsonElement("description"), JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
        
        [BsonElement("to_bank"), JsonPropertyName("to_bank")]
        public string? ToBank { get; set; } = null;
        
        /// <summary>
        /// To Whom the due amount should be credited
        /// </summary>
        [BsonElement("payee"), JsonPropertyName("payee"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? Payee { get; set; } = null;

        [BsonElement("amount"), JsonPropertyName("amount")]
        public double? Amount { get; set; } = null;

        [Required, BsonElement("status"), JsonPropertyName("status")]
        public DueStatus? Status { get; set; } = DueStatus.Active;
    }
}

namespace BudgetTracker.API.Dues
{
    public class DueTransactions
    {
        [BsonElement("description")]
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [BsonElement("amount")]
        [JsonPropertyName("amount")]
        public double Amount { get; set; }

        [BsonElement("status")]
        [JsonPropertyName("status")]
        public DueStatus? Status { get; set; }

        [BsonElement("transactions")]
        [JsonPropertyName("transactions")]
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}

namespace BudgetTracker.Injectors
{
    public interface IDues : IMongoService<Due>
    { 
        public Task InsertOneAsync(Due body);
        public Task<DueTransactions> GetDueTransactionsAsync(string dueId);
    }
}