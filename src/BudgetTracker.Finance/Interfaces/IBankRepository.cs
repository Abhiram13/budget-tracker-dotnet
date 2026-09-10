using BudgetTracker.Entities;

namespace BudgetTracker.Interfaces;

public interface IBankRepository : IMongoDbRepository<Bank> { }