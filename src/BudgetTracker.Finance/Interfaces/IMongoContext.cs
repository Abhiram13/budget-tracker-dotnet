using BudgetTracker.Entities;
using MongoDB.Driver;

namespace BudgetTracker.Interfaces;

public interface IMongoContext
{
    IMongoDatabase Database { get; }
    IMongoCollection<Transaction> Transaction { get; }
    IMongoCollection<Category> Category { get; }
    IMongoCollection<Bank> Bank { get; }
    IMongoCollection<Due> Dues { get; }
}