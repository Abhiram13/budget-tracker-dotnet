using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetTracker.Core.Application.Interfaces;
using BudgetTracker.Core.Domain.Entities;
using BudgetTracker.Core.Domain.Enums;
using BudgetTracker.Core.Domain.ValueObject.Dues;
using MongoDB.Driver;

namespace BudgetTracker.Core.Application.Services;

public class DueService
{
    private readonly IDueRepository _dueRepository;

    public DueService(IDueRepository repository)
    {
        _dueRepository = repository;
    }

    // TODO: Due validations
    public async Task AddOneAsync(Due document)
    {
        await _dueRepository.InserOneAsync(document);
    }

    public async Task<List<Due>> ListAsync(ProjectionDefinition<Due>? exclude = null)
    {
        List<Due> dues = await _dueRepository.GetListAsync(excludeProjection: exclude);
        return dues;
    }

    public async Task<List<DueList>> DueListAsync(DueStatus? dueStatus = null)
    {
        List<DueList> result = await _dueRepository.ListOfDuesAsync(dueStatus);
        return result;
    }

    public async Task<DueDetails> SearchByIdAsync(string id)
    {
        DueDetails due = await _dueRepository.GetDueDetailsAsync(id);
        return due;
    }

    public async Task<bool> UpdateOneAsync(string id, Due body)
    {
        bool isUpdated = await _dueRepository.UpdateByIdAsync(id, body);
        return isUpdated;
    }

    public async Task<bool> DeleteOneAsync(string id)
    {
        bool isDeleted = await _dueRepository.DeleteByIdAsync(id);
        return isDeleted;
    }

    public async Task<List<DueTransactions>> GetDueTransactionsAsync(string dueId)
    {
        return await _dueRepository.GetDueTranasactionsAsync(dueId);
    }
}