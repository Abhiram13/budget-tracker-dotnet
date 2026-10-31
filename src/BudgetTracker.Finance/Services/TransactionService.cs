using System;
using System.Threading;
using System.Threading.Tasks;
using BudgetTracker.Exceptions;
using BudgetTracker.Interfaces;
using BudgetTracker.Entities;
using BudgetTracker.Enums;
using BudgetTracker.Models;
using BudgetTracker.ValueObject.Transaction;
using BudgetTracker.ValueObject.Transaction.List;

using BankResult = BudgetTracker.ValueObject.Transaction.ByBank.ResultByBank;
using CategoryResult = BudgetTracker.ValueObject.Transaction.ByCategory.Result;
using ListResult = BudgetTracker.ValueObject.Transaction.List.Result;

namespace BudgetTracker.Services;

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

    // public async Task<ListResult> ListAsync(QueryParams? queryParams, CancellationToken? cancellationToken)
    // {
    //     ListResult result = await _transactionRepository.ListAsync(
    //         queryParams: queryParams,
    //         cancellationToken: cancellationToken
    //     );
    //
    //     return result;
    // }
    //
    // public async Task<ByDateTransactions> ListByDateAsync(string date)
    // {
    //     ByDateTransactions result = await _transactionRepository.ListByDateAsync(date);
    //     return result;
    // }
    //
    // public async Task<CategoryResult> ListByCategoryAsync(string categoryId, QueryParams queryParams)
    // {
    //     CategoryResult result = await _transactionRepository.GetByCategoryAsync(categoryId, queryParams);
    //     return result;
    // }
    //
    // public async Task<BankResult> ListByBankAsync(string bankId, QueryParams queryParams)
    // {
    //     BankResult result = await _transactionRepository.GetByBankAsync(bankId, queryParams);
    //     return result;
    // }
    //
    // public async Task<Transaction> GetByIdAsync(string id)
    // {
    //     Transaction transaction = await _transactionRepository.SearchByIdAsync(id);
    //     return transaction;
    // }
    //
    public async Task InsertOneAsync(InsertTransactionDto payload)
    {
        bool isCategoryExists = await _categoryRepository.CountByIdAsync(payload.CategoryId);
        
        if (isCategoryExists == false)
        {
            throw new BadRequestException("Invalid Category id provided");
        }
        
        if (string.IsNullOrEmpty(payload.FromBank) && string.IsNullOrEmpty(payload.ToBank))
        {
            throw new BadRequestException("Invalid To bank id and From bank id provided.");
        }
        
        if (payload.Type == TransactionType.Debit)
        {
            if (string.IsNullOrEmpty(payload.FromBank))
            {
                throw new BadRequestException("Invalid From bank id provided for the transaction debit type");
            }
        
            bool isBankExists = await _bankRepository.CountByIdAsync(payload.FromBank);
        
            if (isBankExists == false)
            {
                throw new BadRequestException("Invalid From bank id provided for the transaction debit type");
            }
        }
        
        if (payload.Type == TransactionType.Credit)
        {
            if (string.IsNullOrEmpty(payload.ToBank))
            {
                throw new BadRequestException("Invalid To bank id provided for the transaction credit type");
            }
        
            bool isBankExists = await _bankRepository.CountByIdAsync(payload.ToBank);
        
            if (isBankExists == false)
            {
                throw new BadRequestException("Invalid To bank id provided for the transaction credit type");
            }
        }

        Transaction transaction = Transaction.Create(
            amount: payload.Amount,
            description: payload.Description,
            type: payload.Type,
            fromBank: payload.FromBank,
            toBank: payload.ToBank,
            categoryId: payload.CategoryId,
            date: payload.Date
        );
        
        await _transactionRepository.InsertOneAsync(transaction);
    }
    //
    // public async Task<bool> UpdateOnAsync(string id, Transaction body)
    // {
    //     bool isUpdated = await _transactionRepository.UpdateByIdAsync(id, body);
    //     return isUpdated;
    // }
    //
    // public async Task<bool> DeleteOneAsync(string id)
    // {
    //     bool isDeleted = await _transactionRepository.DeleteByIdAsync(id);
    //     return isDeleted;
    // }
}