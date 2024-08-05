using BudgetTracker.Defination;
using BudgetTracker.Injectors;

namespace BudgetTracker.Services;

public class CategoryService : MongoServices<Category>, ICategoryService
{
    public CategoryService() : base(Collection.Category) { }    
}