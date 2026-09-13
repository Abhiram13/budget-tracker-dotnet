using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using BudgetTracker.Attributes;
using BudgetTracker.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BudgetTracker.Entities;

public class Transaction : MongoObject
{
    [BsonElement("amount")]
    public double Amount { get; private set; }

    [Required]
    [EnumDataType(typeof(TransactionType), ErrorMessage = "Invalid transaction type.")]
    [BsonElement("type")]
    public TransactionType Type { get; private set; } = TransactionType.Debit;

    [Required]
    [RegularExpression(@"^[A-Za-z0-9,.\s]+$", ErrorMessage = "Please provide valid description.")]
    [BsonElement("description")]
    public string Description { get; private set; } = string.Empty;

    [Required]
    [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "Please provide valid date.")]
    [MaxDate(ErrorMessage = "Provided date is out of range or invalid.")]
    [BsonElement("date")]
    public DateOnly Date { get; private set; }

    [BsonRepresentation(BsonType.ObjectId)]
    [BsonElement("from_bank")]
    public string? FromBank { get; private set; } = null;

    [BsonRepresentation(BsonType.ObjectId)]
    [BsonElement("to_bank")]
    public string? ToBank { get; private set; } = null;

    [Required]
    [BsonElement("category_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string CategoryId { get; private set; } = string.Empty;

    [BsonElement("due_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? DueId { get; set; } = null;

    public static Transaction Create(double amount, string description, TransactionType type, string fromBank, string toBank, string categoryId, DateOnly date)
    {
        Transaction transaction = new Transaction
        {
            Amount = amount,
            Description = description,
            FromBank = fromBank,
            ToBank = toBank,
            Type = type,
            CategoryId = categoryId,
            Date = date
        };
        
        transaction.SetModifiedAt();

        return transaction;
    }

    public void Update(double amount, string description, TransactionType type, string fromBank, string toBank, string categoryId, DateOnly date)
    {
        Amount = amount;
        Description = description;
        CategoryId = categoryId;
        FromBank = fromBank;
        ToBank = toBank;
        Date = date;
        Type = type;
        
        SetUpdatedAt();
    }
}