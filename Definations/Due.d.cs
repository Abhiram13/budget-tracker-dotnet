using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace BudgetTracker.Defination
{
    public enum DueStatus
    {
        Completed = 1,
        Pending = 2
    }

    public class Due : MongoObject
    {
        [BsonElement("from")]
        [JsonPropertyName("from")]
        public string From { get; set; } = "";

        [BsonElement("to")]
        [JsonPropertyName("to")]
        public string To { get; set; } = "";

        [BsonElement("transaction_id")]
        [JsonPropertyName("transaction_id")]
        public string TransactionId { get; set; } = "";

        [BsonElement("status")]
        [JsonPropertyName("status")]
        public DueStatus Status { get; set; } = DueStatus.Pending;

        [BsonElement("total_amount")]
        [JsonPropertyName("total_amount")]
        public double TotalAmount { get; set; } = 0;

        [BsonElement("due_amount")]
        [JsonPropertyName("due_amount")]
        public double DueAmount { get; set; } = 0;
    }    
}

namespace BudgetTracker.Injectors
{
    public interface IDueService : IMongoService<Defination.Due> 
    { 
        public void Validate(Defination.Due payload);
    }
}