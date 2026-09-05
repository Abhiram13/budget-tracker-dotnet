using BudgetTracker.Interfaces;
using BudgetTracker.Entities;

namespace BudgetTracker.Repository;

public class CategoryRepository : MongoRepository<Category>, ICategoryRepository
{
    public CategoryRepository(IMongoContext context) : base (context.Category) { }
}