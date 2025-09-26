using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetTracker.Core.Application.Interfaces;
using BudgetTracker.Core.Domain.Entities;
using MongoDB.Driver;

namespace BudgetTracker.Core.Application.Services;

public class BankService
{
    private readonly IBankRepository _bankRepository;

    public BankService(IBankRepository repository)
    {
        _bankRepository = repository;
    }

    public async Task AddOneAsync(Bank document)
    {
        await _bankRepository.InserOneAsync(document);
    }

    public async Task<List<Bank>> ListAsync(ProjectionDefinition<Bank>? exclude = null)
    {
        List<Bank> banks = await _bankRepository.GetListAsync(excludeProjection: exclude);
        return banks;
    }

    public async Task<Bank> SearchByIdAsync(string id)
    {
        Bank bank = await _bankRepository.SearchByIdAsync(id);
        return bank;
    }

    public async Task<bool> UpdateOneAsync(string id, Bank body)
    {
        bool isUpdated = await _bankRepository.UpdateByIdAsync(id, body);
        return isUpdated;
    }

    public async Task<bool> DeleteOneAsync(string id)
    {
        bool isDeleted = await _bankRepository.DeleteByIdAsync(id);
        return isDeleted;
    }
}