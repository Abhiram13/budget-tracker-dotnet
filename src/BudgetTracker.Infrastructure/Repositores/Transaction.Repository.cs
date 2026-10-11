using BudgetTracker.Models.Entities;
using BudgetTracker.Models.Mongo;

namespace BudgetTracker.Infrastructure.Repositories;

public class TransactionRepository : MongoRepository<Transaction>, ITransactionRepository
{
    public TransactionRepository(IMongoContext context) : base(context.Transaction) { }
}
