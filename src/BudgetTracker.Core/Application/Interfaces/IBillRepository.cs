using BudgetTracker.Core.Domain.Entities;

namespace BudgetTracker.Core.Application.Interfaces;

public interface IBillRepository : IMongoDbRepository<Bill> { }