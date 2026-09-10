using BudgetTracker.Interfaces;
using BudgetTracker.Entities;

namespace BudgetTracker.Repository;

public class BankRepository : MongoRepository<Bank>, IBankRepository
{
    public BankRepository(IMongoContext context) : base (context.Bank) { }
}