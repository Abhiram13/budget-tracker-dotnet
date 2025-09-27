using BudgetTracker.Core.Domain.Entities;

namespace BudgetTracker.Core.Application.Interfaces;

public interface ICategoryRepository : IMongoDbRepository<Category> { }