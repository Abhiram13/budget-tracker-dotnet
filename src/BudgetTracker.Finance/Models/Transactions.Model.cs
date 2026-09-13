using BudgetTracker.Enums;

namespace BudgetTracker.Models;

public record InsertTransactionDto
{
    [JsonPropertyName("amount")]
    public double Amount { get; set; }
    
    [JsonPropertyName("description")]
    public string Description { get; set; }
    
    [JsonPropertyName("date")]
    public DateOnly Date { get; set; }
    
    [JsonPropertyName("from_bank")]
    public string? FromBank { get; set; }
    
    [JsonPropertyName("to_bank")]
    public string? ToBank { get; set; }
    
    [JsonPropertyName("category_id")]
    public string CategoryId { get; set; }
    
    [JsonPropertyName("type")]
    public TransactionType Type { get; set; }
    
    [JsonPropertyName("due_id")]
    public string? DueId { get; set; }
}