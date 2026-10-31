using BudgetTracker.Models.Entities;

namespace BudgetTracker.Models.Mongo;

public interface ITransactionRepository
{
    public Task InsertOneAsync(Transaction payload);
}
