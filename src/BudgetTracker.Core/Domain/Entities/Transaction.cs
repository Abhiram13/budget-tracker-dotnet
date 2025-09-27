using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using BudgetTracker.Core.Domain.Attributes;
using BudgetTracker.Core.Domain.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace BudgetTracker.Core.Domain.Entities;

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

    [BsonElement("due_id")]
    [JsonPropertyName("due_id")]
    public string? DueId { get; set; } = null;
}