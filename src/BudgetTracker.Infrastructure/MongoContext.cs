using BudgetTracker.Models.Entities;
using BudgetTracker.Models.Mongo;
using MongoDB.Driver;

namespace BudgetTracker.Infrastructure;

internal static class MongoCollections
{
    internal static readonly string Transactions = nameof(Transactions);
    internal static readonly string Categories = nameof(Categories);
    internal static readonly string Banks = nameof(Banks);
    internal static readonly string Dues = nameof(Dues);
}

public class MongoDbContext : IMongoContext
{
    public MongoDbContext(MongoSecrets secrets)
    {
        string url = $"mongodb+srv://{secrets.Username}:{secrets.Password}@{secrets.Host}/?appName={secrets.AppName}";
        using (MongoClient client = new MongoClient(url))
        {
            Database = client.GetDatabase(secrets.Database);
        }
    }

    public IMongoDatabase Database { get; }
    public IMongoCollection<Transaction> Transaction
    {
        get { return Database.GetCollection<Transaction>(MongoCollections.Transactions); }
    }
}
