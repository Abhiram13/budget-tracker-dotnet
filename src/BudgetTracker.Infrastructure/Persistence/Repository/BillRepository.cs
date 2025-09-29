using BudgetTracker.Core.Application.Interfaces;
using BudgetTracker.Core.Domain.Entities;

namespace BudgetTracker.Infrastructure.Repository;

public class BillRepository : MongoRepository<Bill>, IBillRepository
{
    public BillRepository(IMongoContext context) : base (context.Bills) { }
}