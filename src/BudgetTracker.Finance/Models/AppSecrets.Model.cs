using System.ComponentModel.DataAnnotations;

namespace BudgetTracker.Models;

public record AppSecrets
{
    [Required]
    public required string Port { get; init; }
    public string Apikey { get; set; } = string.Empty;
}