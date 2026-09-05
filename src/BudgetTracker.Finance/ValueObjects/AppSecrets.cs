namespace BudgetTracker.Core.Domain.ValueObject;

public class AppSecrets
{
    public required string UserName { get; set; }
    public required string PassWord { get; set; }
    public required string Host { get; set; }
    public required string DataBase { get; set; }
    public required string ApiKey { get; set; }
}