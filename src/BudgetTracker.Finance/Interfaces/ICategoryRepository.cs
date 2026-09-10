using BudgetTracker.Entities;

namespace BudgetTracker.Interfaces;

public interface ICategoryRepository : IMongoDbRepository<Category> { }