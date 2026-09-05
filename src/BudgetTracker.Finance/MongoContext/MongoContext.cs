using BudgetTracker.Interfaces;
using BudgetTracker.Enums;
using BudgetTracker.Entities;
using BudgetTracker.ValueObject;
using MongoDB.Driver;

namespace BudgetTracker.Context;

public class MongoDBContext : IMongoContext
{
    private readonly IMongoDatabase _database;
    private readonly AppSecrets _secrets;

    public MongoDBContext(AppSecrets secrets)
    {
        _secrets = secrets;
        string url = $"mongodb+srv://{_secrets.UserName}:{_secrets.PassWord}@{_secrets.Host}/?retryWrites=true&w=majority&appName=Trsnactions";
        MongoClient client = new MongoClient(url);
        _database = client.GetDatabase(_secrets.DataBase);
    }

    public IMongoDatabase Database => _database;
    public IMongoCollection<Transaction> Transaction => _database.GetCollection<Transaction>(Collection.TRANSACTIONS);
    public IMongoCollection<Category> Category => _database.GetCollection<Category>(Collection.CATEGORIES);
    public IMongoCollection<Bank> Bank => _database.GetCollection<Bank>(Collection.BANKS);
    public IMongoCollection<Due> Dues => _database.GetCollection<Due>(Collection.DUES);
}