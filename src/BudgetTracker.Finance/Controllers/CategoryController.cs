using System.ComponentModel.DataAnnotations;
using System.Net;
using BudgetTracker.Exceptions;
using BudgetTracker.Services;
using BudgetTracker.Entities;
using BudgetTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace BudgetTracker.Controllers;

[ApiController]
public class CategoriesController : ApiBaseController
{
    private readonly CategoryService _service;
    private readonly IMemoryCache _cache;
    private readonly string _cacheKey = "category_cache";

    public CategoriesController(CategoryService service, IMemoryCache cache)
    {
        _service = service;
        _cache = cache;
    }

    [HttpPost]
    public async Task<IActionResult> AddOneAsync([FromBody, Required] InsertCategoryDto body)
    {
        await _service.AddOneAsync(body);
        return StatusCode(StatusCodes.Status201Created, new ApiResponse { Message = "Category inserted successfully", StatusCode = HttpStatusCode.Created});
    }

    // FIXME: Cache should be updated on creation/ deletion of categories
    [HttpGet]
    public async Task<ValueObject.ApiResponse<List<Category>>> GetListAsync()
    {
        // if (!_cache.TryGetValue(_cacheKey, out List<Category>? categories))
        // {
        //     categories = await _service.ListAsync();
        //     _cache.Set(_cacheKey, categories);
        // }

        List<Category> categories = await _service.ListAsync();

        return new ValueObject.ApiResponse<List<Category>>()
        {
            Result = categories,
            StatusCode = HttpStatusCode.OK,
        };
    }

    [HttpGet("{id}")]
    public async Task<ValueObject.ApiResponse<Category>> SearchByIdAsync(string id)
    {
        if (string.IsNullOrEmpty(id)) { throw new BadRequestException("Category id is missing"); }

        Category category = await _service.SearchByIdAsync(id);
        return new ValueObject.ApiResponse<Category>()
        {
            StatusCode = HttpStatusCode.OK,
            Result = category
        };
    }

    [HttpPatch("{id}")]
    public async Task<ValueObject.ApiResponse<string>> UpdateOneAsync(string id, [FromBody] Category body)
    {
        bool isUpdated = await _service.UpdateOneAsync(id, body);
        HttpStatusCode statusCode = isUpdated ? HttpStatusCode.Created : HttpStatusCode.NotModified;
        string message = isUpdated ? "Category updated successfully" : "Category couldn't be updated";

        return new ValueObject.ApiResponse<string>()
        {
            Message = message,
            StatusCode = statusCode,
        };
    }

    [HttpDelete("{id}")]
    public async Task<ValueObject.ApiResponse<string>> DeleteOneAsync(string id)
    {
        bool isDeleted = await _service.DeleteOneAsync(id);
        string message = isDeleted ? "Category deleted successfully" : "Cannot delete selected category";
        HttpStatusCode statusCode = isDeleted ? HttpStatusCode.OK : HttpStatusCode.NotModified;

        return new ValueObject.ApiResponse<string>()
        {
            Message = message,
            StatusCode = statusCode,
        };
    }
}