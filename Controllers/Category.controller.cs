using Microsoft.AspNetCore.Mvc;
using System.Net;
using Microsoft.Extensions.Caching.Memory;
using BudgetTracker.Injectors;
using BudgetTracker.Defination;
using BudgetTracker.Services;

namespace BudgetTracker.Controllers;

[ApiController]
public class CategoryController : ApiBaseController
{
    private readonly ICategoryService _service;
    private readonly IMemoryCache _cache;
    private readonly ILogger _logger;
    private readonly string _cacheKey = "category_cache";

    public CategoryController(ICategoryService service, IMemoryCache memoryCache, ILogger<CategoryController> logger)
    {
        _service = service;
        _cache = memoryCache;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ApiResponse<string>> Add([FromBody] Category body)
    {
        AsyncCallback<string> callback = async () => {
            Category category = body;
            await _service.InserOne(category);
            return new ApiResponse<string>()
            {
                Message = "Category inserted successfully",
                StatusCode = HttpStatusCode.Created,
            };
        };

        return await Handler<string>.Exception(callback, _logger);
    }

    [HttpGet("{id}")]
    public async Task<ApiResponse<Category>> SearchById(string id)
    {
        AsyncCallback<Category> callback = async () => {
            Category category = await _service.SearchById(id);
            return new ApiResponse<Category>()
            {
                StatusCode = HttpStatusCode.OK,
                Result = category
            };
        };

        return await Handler<Category>.Exception(callback, _logger);
    }

    [HttpGet]
    public async Task<ApiResponse<List<Category>>> GetList()
    {
        AsyncCallback<List<Category>> callback = async () => {
            if (!_cache.TryGetValue(_cacheKey, out List<Category> categories))
            {
                categories = await _service.GetList();
                _cache.Set(_cacheKey, categories);
            }

            return new ApiResponse<List<Category>>()
            {
                Result = categories,
                StatusCode = HttpStatusCode.OK,
            };
        };

        return await Handler<List<Category>>.Exception(callback, _logger);
    }

    [HttpPatch("{id}")]
    public async Task<ApiResponse<string>> Update(string id, [FromBody] dynamic body)
    {
        AsyncCallback<string> callback = async () => {
            bool isUpdated = await _service.UpdateById(id, body);
            HttpStatusCode statusCode = isUpdated ? HttpStatusCode.Created : HttpStatusCode.NotModified;
            string message = isUpdated ? "Category updated successfully" : "Category couldn't be updated";

            return new ApiResponse<string>()
            {
                Message = message,
                StatusCode = statusCode,
            };
        };

        return await Handler<string>.Exception(callback, _logger);
    }

    [HttpDelete("{id}")]
    public async Task<ApiResponse<string>> Delete(string id)
    {
        AsyncCallback<string> callback = async () => {
            bool isDeleted = await _service.DeleteById(id);
            string message = isDeleted ? "Category deleted successfully" : "Cannot delete selected category";
            HttpStatusCode statusCode = isDeleted ? HttpStatusCode.OK : HttpStatusCode.NotModified;

            return new ApiResponse<string>()
            {
                Message = message,
                StatusCode = statusCode,
            };
        };

        return await Handler<string>.Exception(callback, _logger);
    }
}