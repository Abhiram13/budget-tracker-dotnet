using BudgetTracker.Core.Application.Interfaces;
using BudgetTracker.Core.Domain.Entities;

namespace BudgetTracker.Infrastructure.Repository;

public class CategoryRepository : MongoRepository<Category>, ICategoryRepository
{
    public CategoryRepository(IMongoContext context) : base (context.Category) { }
}