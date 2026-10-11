using BudgetTracker.Models.Entities;
using MongoDB.Driver;

namespace BudgetTracker.Models.Mongo;

public interface IMongoContext
{
    public IMongoDatabase Database { get; }
    public IMongoCollection<Transaction> Transaction { get; }
    // IMongoCollection<Category> Category { get; }
    // IMongoCollection<Bank> Bank { get; }
    // IMongoCollection<Due> Dues { get; }
}
