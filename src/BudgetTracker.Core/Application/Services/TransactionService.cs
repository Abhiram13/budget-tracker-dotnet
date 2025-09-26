using System;
using System.Threading;
using System.Threading.Tasks;
using BudgetTracker.Core.Application.Exceptions;
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
    private readonly ICategoryRepository _categoryRepository;
    private readonly IBankRepository _bankRepository;

    public TransactionService(ITransactionRepository repository, ICategoryRepository categoryRepository, IBankRepository bankRepository)
    {
        _transactionRepository = repository;
        _categoryRepository = categoryRepository;
        _bankRepository = bankRepository;
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
        Category category = await _categoryRepository.SearchByIdAsync(doc.CategoryId);

        if (category is null || string.IsNullOrEmpty(category.Name))
        {
            throw new BadRequestException($"Invalid Category Id ({0}) provided", doc.CategoryId);
        }

        if (string.IsNullOrEmpty(doc.FromBank) && string.IsNullOrEmpty(doc.ToBank))
        {
            throw new BadRequestException($"Invalid From Bank ({0}) and To Bank ({1}) provided", doc.FromBank, doc.ToBank);
        }

        Func<string?, Task> ValidateBanks = async (string? bankId) =>
        {
            if (string.IsNullOrEmpty(bankId)) return;

            Bank bank = await _bankRepository.SearchByIdAsync(bankId);

            if (bank is null || string.IsNullOrEmpty(bank.Name))
            {
                throw new BadRequestException($"Invalid bank id ({0}) provided", bankId);
            }
        };

        await ValidateBanks(doc.FromBank);
        await ValidateBanks(doc.ToBank);
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