using BudgetTracker.Core.Application.Interfaces;
using BudgetTracker.Core.Domain.Entities;

namespace BudgetTracker.Infrastructure.Repository;

public class DueRepository : MongoRepository<Due>, IDueRepository
{
    public DueRepository(IMongoContext context) : base (context.Dues) { }
}