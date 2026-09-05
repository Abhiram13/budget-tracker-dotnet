using BudgetTracker.Interfaces;
using BudgetTracker.Entities;

namespace BudgetTracker.Repository;

public class DueRepository : MongoRepository<Due>, IDueRepository
{
    public DueRepository(IMongoContext context) : base (context.Dues) { }
}