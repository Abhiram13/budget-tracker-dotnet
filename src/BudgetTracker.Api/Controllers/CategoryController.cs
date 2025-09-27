using System.Net;
using BudgetTracker.Core.Application.Exceptions;
using BudgetTracker.Core.Application.Services;
using BudgetTracker.Core.Domain.Entities;
using BudgetTracker.Core.Domain.ValueObject;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace BudgetTracker.Api.Controllers;

[ApiController]
public class CategoryController : ApiBaseController
{
    private readonly CategoryService _service;
    private readonly IMemoryCache _cache;
    private readonly string _cacheKey = "category_cache";

    public CategoryController(CategoryService service, IMemoryCache cache)
    {
        _service = service;
        _cache = cache;
    }

    public async Task<ApiResponse<string>> AddOneAsync([FromBody] Category body)
    {
        await _service.AddOneAsync(body);
        return new ApiResponse<string>()
        {
            Message = "Category inserted successfully",
            StatusCode = HttpStatusCode.Created,
        };
    }

    [HttpGet]
    public async Task<ApiResponse<List<Category>>> GetListAsync()
    {
        if (!_cache.TryGetValue(_cacheKey, out List<Category>? categories))
        {
            categories = await _service.ListAsync();
            _cache.Set(_cacheKey, categories);
        }

        return new ApiResponse<List<Category>>()
        {
            Result = categories,
            StatusCode = HttpStatusCode.OK,
        };
    }

    [HttpGet("{id}")]
    public async Task<ApiResponse<Category>> SearchByIdAsync(string id)
    {
        if (string.IsNullOrEmpty(id)) { throw new BadRequestException("Category id is missing"); }

        Category category = await _service.SearchByIdAsync(id);
        return new ApiResponse<Category>()
        {
            StatusCode = HttpStatusCode.OK,
            Result = category
        };
    }

    [HttpPatch("{id}")]
    public async Task<ApiResponse<string>> UpdateOneAsync(string id, [FromBody] Category body)
    {
        bool isUpdated = await _service.UpdateOneAsync(id, body);
        HttpStatusCode statusCode = isUpdated ? HttpStatusCode.Created : HttpStatusCode.NotModified;
        string message = isUpdated ? "Category updated successfully" : "Category couldn't be updated";

        return new ApiResponse<string>()
        {
            Message = message,
            StatusCode = statusCode,
        };
    }

    [HttpDelete("{id}")]
    public async Task<ApiResponse<string>> DeleteOneAsync(string id)
    {
        bool isDeleted = await _service.DeleteOneAsync(id);
        string message = isDeleted ? "Category deleted successfully" : "Cannot delete selected category";
        HttpStatusCode statusCode = isDeleted ? HttpStatusCode.OK : HttpStatusCode.NotModified;

        return new ApiResponse<string>()
        {
            Message = message,
            StatusCode = statusCode,
        };
    }
}