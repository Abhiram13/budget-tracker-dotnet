namespace BudgetTracker.Models;

public record InsertBankDto
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }
}