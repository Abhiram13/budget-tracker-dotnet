using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetTracker.Interfaces;
using BudgetTracker.Entities;
using MongoDB.Driver;

namespace BudgetTracker.Services;

public class DueService
{
    private readonly IDueRepository _dueRepository;

    public DueService(IDueRepository repository)
    {
        _dueRepository = repository;
    }

    public async Task AddOneAsync(Due document)
    {
        await _dueRepository.InserOneAsync(document);
    }

    public async Task<List<Due>> ListAsync(ProjectionDefinition<Due>? exclude = null)
    {
        List<Due> dues = await _dueRepository.GetListAsync(excludeProjection: exclude);
        return dues;
    }

    public async Task<Due> SearchByIdAsync(string id)
    {
        Due due = await _dueRepository.SearchByIdAsync(id);
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
}