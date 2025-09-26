using System.Threading;
using System.Threading.Tasks;
using BudgetTracker.Core.Application.Interfaces;
using BudgetTracker.Core.Domain.Entities;
using BudgetTracker.Core.Domain.ValueObject.Transaction;
using BudgetTracker.Core.Domain.ValueObject.Transaction.List;

using BankResult = BudgetTracker.Core.Domain.ValueObject.Transaction.ByBank.ResultByBank;
using CategoryResult = BudgetTracker.Core.Domain.ValueObject.Transaction.ByCategory.Result;
using ListResult = BudgetTracker.Core.Domain.ValueObject.Transaction.List.Result;

namespace BudgetTracker.Core.Application.Services;

public class TransactionService
{
    private readonly ITransactionRepository _transactionRepository;

    public TransactionService(ITransactionRepository repository)
    {
        _transactionRepository = repository;
    }

    public async Task<ListResult> ListAsync(QueryParams? queryParams, CancellationToken? cancellationToken)
    {
        ListResult result = await _transactionRepository.ListAsync(
            queryParams: queryParams,
            cancellationToken: cancellationToken
        );

        return result;
    }

    public async Task<ByDateTransactions> ListByDateAsync(string date)
    {
        ByDateTransactions result = await _transactionRepository.ListByDateAsync(date);
        return result;
    }

    public async Task<CategoryResult> ListByCategoryAsync(string categoryId, QueryParams queryParams)
    {
        CategoryResult result = await _transactionRepository.GetByCategoryAsync(categoryId, queryParams);
        return result;
    }

    public async Task<BankResult> ListByBankAsync(string bankId, QueryParams queryParams)
    {
        BankResult result = await _transactionRepository.GetByBankAsync(bankId, queryParams);
        return result;
    }

    public async Task<Transaction> GetByIdAsync(string id)
    {
        Transaction transaction = await _transactionRepository.SearchByIdAsync(id);
        return transaction;
    }

    public async Task InsertOneAsync(Transaction doc)
    {
        await _transactionRepository.InserOneAsync(doc);
    }

    public async Task<bool> UpdateOnAsync(string id, Transaction body)
    {
        bool isUpdated = await _transactionRepository.UpdateByIdAsync(id, body);
        return isUpdated;
    }

    public async Task<bool> DeleteOneAsync(string id)
    {
        bool isDeleted = await _transactionRepository.DeleteByIdAsync(id);
        return isDeleted;
    }
}