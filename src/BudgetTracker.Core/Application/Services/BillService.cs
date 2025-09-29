using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetTracker.Core.Application.Interfaces;
using BudgetTracker.Core.Domain.Entities;
using MongoDB.Driver;

namespace BudgetTracker.Core.Application.Services;

public class BillService
{
    private readonly IBillRepository _billRepository;

    public BillService(IBillRepository repository)
    {
        _billRepository = repository;
    }

    public async Task AddOneAsync(Bill document)
    {
        await _billRepository.InserOneAsync(document);
    }

    public async Task<List<Bill>> ListAsync(ProjectionDefinition<Bill>? exclude = null)
    {
        List<Bill> bills = await _billRepository.GetListAsync(excludeProjection: exclude);
        return bills;
    }

    public async Task<Bill> SearchByIdAsync(string id)
    {
        Bill bill = await _billRepository.SearchByIdAsync(id);
        return bill;
    }

    public async Task<bool> UpdateOneAsync(string id, Bill body)
    {
        bool isUpdated = await _billRepository.UpdateByIdAsync(id, body);
        return isUpdated;
    }

    public async Task<bool> DeleteOneAsync(string id)
    {
        bool isDeleted = await _billRepository.DeleteByIdAsync(id);
        return isDeleted;
    }
}