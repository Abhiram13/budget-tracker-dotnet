namespace BudgetTracker.Models;

public record InsertCategoryDto
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }
}