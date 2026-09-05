using System.Threading;
using System.Threading.Tasks;
using BudgetTracker.Core.Domain.Entities;
using BudgetTracker.Core.Domain.ValueObject.Transaction;
using BudgetTracker.Core.Domain.ValueObject.Transaction.List;

using BankResult = BudgetTracker.Core.Domain.ValueObject.Transaction.ByBank.ResultByBank;
using CategoryResult = BudgetTracker.Core.Domain.ValueObject.Transaction.ByCategory.Result;
using ListResult = BudgetTracker.Core.Domain.ValueObject.Transaction.List.Result;

namespace BudgetTracker.Core.Application.Interfaces;

public interface ITransactionRepository : IMongoDbRepository<Transaction>
{
    Task<ListResult> ListAsync(QueryParams? queryParams, CancellationToken? cancellationToken = default);
    Task<ByDateTransactions> ListByDateAsync(string date);
    Task<CategoryResult> GetByCategoryAsync(string categoryId, QueryParams queryParams);
    Task<BankResult> GetByBankAsync(string bankId, QueryParams queryParams);
}