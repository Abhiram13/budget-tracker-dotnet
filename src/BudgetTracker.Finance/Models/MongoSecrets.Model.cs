using System.ComponentModel.DataAnnotations;

namespace BudgetTracker.Models;

public record MongoSecrets
{
    [Required]
    public required string Username { get; init; }
    
    [Required]
    public required string Password { get; init; }
    
    [Required]
    public required string Host { get; init; }
    
    [Required]
    public required string Database { get; init; }
}