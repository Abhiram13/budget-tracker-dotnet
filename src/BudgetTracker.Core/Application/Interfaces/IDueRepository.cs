using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetTracker.Core.Domain.Entities;
using BudgetTracker.Core.Domain.Enums;
using BudgetTracker.Core.Domain.ValueObject.Dues;

namespace BudgetTracker.Core.Application.Interfaces;

public interface IDueRepository : IMongoDbRepository<Due>
{
    Task<List<DueList>> ListOfDuesAsync(DueStatus? dueStatus = null);
    Task<List<DueTransactions>> GetDueTranasactionsAsync(string id);
    Task<DueDetails> GetDueDetailsAsync(string id);
}