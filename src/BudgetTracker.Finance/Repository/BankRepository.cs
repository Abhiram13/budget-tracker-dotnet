using BudgetTracker.Core.Application.Interfaces;
using BudgetTracker.Core.Domain.Entities;

namespace BudgetTracker.Infrastructure.Repository;

public class BankRepository : MongoRepository<Bank>, IBankRepository
{
    public BankRepository(IMongoContext context) : base (context.Bank) { }
}