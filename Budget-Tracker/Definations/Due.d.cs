using System.ComponentModel.DataAnnotations;
using System.Net;
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

    public enum DueType
    {
        /// <summary>
        /// Dues that are paid for the fixed amount of time. Example: E.M.I
        /// </summary>
        Fixed = 1,

        /// <summary>
        /// Dues paid whenever possible until the full amount is settled. For example, owing money to a friend.
        /// </summary>
        NonFixed = 2,
    }

    public class Due : MongoObject
    {
        /// <summary>
        /// To Whom the due amount should be credited
        /// </summary>
        [Required, BsonElement("payee"), JsonPropertyName("payee"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string Payee { get; init; } = string.Empty;

        /// <summary>
        /// The initial amount borrowed or the original debt, before interest or fees.
        /// </summary>
        [Required, BsonElement("principal_amount"), JsonPropertyName("principal_amount")]
        public double PrincipalAmount { get; init; }

        /// <summary>
        /// Generic description of what the due amount for
        /// </summary>
        [Required, BsonElement("description"), JsonPropertyName("description")]
        public string Description { get; init; } = string.Empty;

        [Required, BsonElement("start_date"), JsonPropertyName("start_date"), BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime StartDate { get; set; }

        [BsonElement("end_date"), JsonPropertyName("end_date"), BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Field for additional notes or descriptions.
        /// </summary>
        [BsonElement("comment"), JsonPropertyName("comment")]
        public string Comment { get; init; } = string.Empty;

        /// <summary>
        /// The remaining balance of the original loan or debt, excluding any accrued interest or fees. <br />
        /// This value decreases as principal payments are made.
        /// </summary>
        [BsonIgnoreIfDefault, BsonElement("current_principal"), JsonPropertyName("current_principal"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public double CurrentPrincipal { get; set; } = 0;

        /// <summary>
        /// Status of the Due:
        /// <list type="bullet">
        ///     <item><description>Active</description></item>
        ///     <item><description>Ended</description></item>
        ///     <item><description>Hold</description></item>
        ///     <item><description>InProgress</description></item>
        ///     <item><description>InActive</description></item>
        /// </list>
        /// </summary>
        [Required, BsonElement("status"), JsonPropertyName("status")]
        public DueStatus Status { get; set; } = DueStatus.Active;

        [BsonElement("to_bank"), JsonPropertyName("to_bank")]
        public string ToBank { get; set; } = string.Empty;

        /// <summary>
        /// Specified type of Due:
        /// <list type="bullet">
        /// <item>Fixed: <description>For regular, scheduled payments (like E.M.Is)</description></item>
        /// <item>Non Fixed: <description>For irregular payments until a total amount is cleared.</description></item>
        /// </list>
        /// </summary>
        [Required, BsonElement("type"), JsonPropertyName("type")]
        public DueType Type { get; set; } = DueType.NonFixed;
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
        [BsonElement("current_principal")]
        [JsonPropertyName("current_principal")]
        public double CurrentPrincipal { get; set; }

        [BsonElement("transactions")]
        [JsonPropertyName("transactions")]
        public List<Transactions> Transactions { get; set; } = new List<Transactions>();
    }

    public class DueList
    {
        [BsonElement("description"), JsonPropertyName("description")]
        public string Description { get; init; } = string.Empty;

        [BsonElement("id"), JsonPropertyName("id")]
        public string Id { get; init; } = string.Empty;
    }

    public enum DueDeleteResult
    {
        OK = 200,
        ERROR = 500,
        NOT_MODIFIED = 300,
        DOC_EXIST = 400,  
    }
}

namespace BudgetTracker.Interface
{
    public interface IDues : IMongoService<Due>
    {
        public Task InsertOneAsync(Due body);
        public Task Validations(Due due);
        public Task<DueTransactions> GetDueTransactionsAsync(string dueId);
        public Task<List<DueList>> ListAsync(string? id);
        public Task<Due> GetByIdAsync(string id);
        public Task<HttpStatusCode> DeleteByIdAsync(string id);
    }
}