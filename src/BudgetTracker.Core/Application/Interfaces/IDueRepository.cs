using BudgetTracker.Core.Domain.Entities;

namespace BudgetTracker.Core.Application.Interfaces;

public interface IDueRepository : IMongoDbRepository<Due> { }