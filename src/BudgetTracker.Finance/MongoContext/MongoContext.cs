using BudgetTracker.Interfaces;
using BudgetTracker.Constants;
using BudgetTracker.Entities;
using BudgetTracker.Models;
using BudgetTracker.ValueObject;
using MongoDB.Driver;

namespace BudgetTracker.Context;

public class MongoDBContext : IMongoContext
{
    private readonly IMongoDatabase _database;

    public MongoDBContext(MongoSecrets secrets)
    {
        string url = $"mongodb+srv://{secrets.Username}:{secrets.Password}@{secrets.Host}/?appName={secrets.AppName}";
        MongoClient client = new MongoClient(url);
        _database = client.GetDatabase(secrets.Database);
    }

    public IMongoDatabase Database => _database;
    public IMongoCollection<Transaction> Transaction => _database.GetCollection<Transaction>(MongoCollections.Transactions);
    public IMongoCollection<Category> Category => _database.GetCollection<Category>(MongoCollections.Categories);
    public IMongoCollection<Bank> Bank => _database.GetCollection<Bank>(MongoCollections.Banks);
    public IMongoCollection<Due> Dues => _database.GetCollection<Due>(MongoCollections.Dues);
}