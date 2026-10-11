using BudgetTracker.Entities;
using MongoDB.Driver;

namespace BudgetTracker.Interfaces;

[Obsolete(message: "use from BudgetTracker.Models", error: true)]
public interface IMongoContext
{
    IMongoDatabase Database { get; }
    // IMongoCollection<Transaction> Transaction { get; }
    IMongoCollection<Category> Category { get; }
    IMongoCollection<Bank> Bank { get; }
    IMongoCollection<Due> Dues { get; }
}
