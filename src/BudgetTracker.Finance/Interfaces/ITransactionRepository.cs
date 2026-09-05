using System.Threading;
using System.Threading.Tasks;
using BudgetTracker.Entities;
using BudgetTracker.ValueObject.Transaction;
using BudgetTracker.ValueObject.Transaction.List;

using BankResult = BudgetTracker.ValueObject.Transaction.ByBank.ResultByBank;
using CategoryResult = BudgetTracker.ValueObject.Transaction.ByCategory.Result;
using ListResult = BudgetTracker.ValueObject.Transaction.List.Result;

namespace BudgetTracker.Interfaces;

public interface ITransactionRepository : IMongoDbRepository<Transaction>
{
    Task<ListResult> ListAsync(QueryParams? queryParams, CancellationToken? cancellationToken = default);
    Task<ByDateTransactions> ListByDateAsync(string date);
    Task<CategoryResult> GetByCategoryAsync(string categoryId, QueryParams queryParams);
    Task<BankResult> GetByBankAsync(string bankId, QueryParams queryParams);
}