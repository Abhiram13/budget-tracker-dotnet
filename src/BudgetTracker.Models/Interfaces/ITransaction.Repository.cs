using BudgetTracker.Models.Entities;

namespace BudgetTracker.Models.Mongo;

public interface ITransactionRepository : IMongoRepository<Transaction>;
