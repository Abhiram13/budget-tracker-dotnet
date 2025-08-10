using Microsoft.AspNetCore.Mvc;
using System.Net;
using Microsoft.Extensions.Caching.Memory;
using BudgetTracker.Interface;
using BudgetTracker.Defination;
using BudgetTracker.Services;
using BudgetTracker.Application;

namespace BudgetTracker.Controllers;

[ApiController]
public class CategoryController : ApiBaseController
{
    private readonly ICategoryService _service;
    private readonly IMemoryCache _cache;
    private readonly string _cacheKey = "category_cache";

    public CategoryController(ICategoryService service, IMemoryCache memoryCache)
    {
        _service = service;
        _cache = memoryCache;
    }

    [HttpPost]
    public async Task<ApiResponse<string>> Add([FromBody] Category body)
    {
        Category category = body;
        await _service.InserOne(category);
        return new ApiResponse<string>()
        {
            Message = "Category inserted successfully",
            StatusCode = HttpStatusCode.Created,
        };
    }

    [HttpGet("{id}")]
    public async Task<ApiResponse<Category>> SearchById(string id)
    {
        if (string.IsNullOrEmpty(id)) { throw new BadRequestException("Category id is missing"); }
                        
        Category category = await _service.SearchById(id);
        return new ApiResponse<Category>()
        {
            StatusCode = HttpStatusCode.OK,
            Result = category
        };
    }

    [HttpGet]
    public async Task<ApiResponse<List<Category>>> GetList()
    {
        if (!_cache.TryGetValue(_cacheKey, out List<Category>? categories))
        {
            categories = await _service.GetList();
            _cache.Set(_cacheKey, categories);
        }

        return new ApiResponse<List<Category>>()
        {
            Result = categories,
            StatusCode = HttpStatusCode.OK,
        };
    }

    [HttpPatch("{id}")]
    public async Task<ApiResponse<string>> Update(string id, [FromBody] Category body)
    {
        bool isUpdated = await _service.UpdateById(id, body);
        HttpStatusCode statusCode = isUpdated ? HttpStatusCode.Created : HttpStatusCode.NotModified;
        string message = isUpdated ? "Category updated successfully" : "Category couldn't be updated";

        return new ApiResponse<string>()
        {
            Message = message,
            StatusCode = statusCode,
        };
    }

    [HttpDelete("{id}")]
    public async Task<ApiResponse<string>> Delete(string id)
    {
        bool isDeleted = await _service.DeleteById(id);
        string message = isDeleted ? "Category deleted successfully" : "Cannot delete selected category";
        HttpStatusCode statusCode = isDeleted ? HttpStatusCode.OK : HttpStatusCode.NotModified;

        return new ApiResponse<string>()
        {
            Message = message,
            StatusCode = statusCode,
        };
    }
}