using BudgetTracker.Defination;
using BudgetTracker.Interface;

namespace BudgetTracker.Services;

public class CategoryService : MongoServices<Category>, ICategoryService
{
    public CategoryService() : base(Collection.Category) { }    
}