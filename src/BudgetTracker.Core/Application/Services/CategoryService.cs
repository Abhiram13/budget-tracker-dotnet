using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetTracker.Core.Application.Interfaces;
using BudgetTracker.Core.Domain.Entities;
using MongoDB.Driver;

namespace BudgetTracker.Core.Application.Services;

public class CategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository repository)
    {
        _categoryRepository = repository;
    }

    public async Task AddOneAsync(Category document)
    {
        await _categoryRepository.InserOneAsync(document);
    }

    public async Task<List<Category>> ListAsync(ProjectionDefinition<Category>? exclude = null)
    {
        List<Category> categories = await _categoryRepository.GetListAsync(excludeProjection: exclude);
        return categories;
    }

    public async Task<Category> SearchByIdAsync(string id)
    {
        Category category = await _categoryRepository.SearchByIdAsync(id);
        return category;
    }

    public async Task<bool> UpdateOneAsync(string id, Category body)
    {
        bool isUpdated = await _categoryRepository.UpdateByIdAsync(id, body);
        return isUpdated;
    }

    public async Task<bool> DeleteOneAsync(string id)
    {
        bool isDeleted = await _categoryRepository.DeleteByIdAsync(id);
        return isDeleted;
    }
}