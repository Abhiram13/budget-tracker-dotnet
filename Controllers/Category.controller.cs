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
    private readonly ICategoryService service;
    private readonly IMemoryCache cache;
    private readonly string cacheKey = "category_cache";

    public CategoryController(ICategoryService _service, IMemoryCache memoryCache)
    {
        service = _service;
        cache = memoryCache;        
    }

    [HttpPost]
    public async Task<ApiResponse<string>> Add([FromBody] Category body)
    {
        AsyncCallback<string> callback = async () => {
            Category category = body;
            await service.InserOne(category);
            return new ApiResponse<string>()
            {
                Message = "Category inserted successfully",
                StatusCode = HttpStatusCode.Created,
            };
        };

        return await Handler<string>.Exception(callback);
    }

    [HttpGet("{id}")]
    public async Task<ApiResponse<Category>> SearcById(string id)
    {
        AsyncCallback<Category> callback = async () => {
            Category category = await service.SearchById(id);
            return new ApiResponse<Category>()
            {
                StatusCode = HttpStatusCode.OK,
                Result = category
            };
        };

        return await Handler<Category>.Exception(callback);
    }

    [HttpGet]
    public async Task<ApiResponse<List<Category>>> GetList()
    {
        AsyncCallback<List<Category>> callback = async () => {
            if (!cache.TryGetValue(cacheKey, out List<Category> categories))
            {
                categories = await service.GetList();
                cache.Set(cacheKey, categories);
            }

            return new ApiResponse<List<Category>>()
            {
                Result = categories,
                StatusCode = HttpStatusCode.OK,
            };
        };

        return await Handler<List<Category>>.Exception(callback);
    }

    [HttpPatch("{id}")]
    public async Task<ApiResponse<string>> Update(string id, [FromBody] dynamic body)
    {
        AsyncCallback<string> callback = async () => {
            bool isUpdated = await service.UpdateById(id, body);
            HttpStatusCode statusCode = isUpdated ? HttpStatusCode.Created : HttpStatusCode.NotModified;
            string message = isUpdated ? "Category updated successfully" : "Category couldn't be updated";

            return new ApiResponse<string>()
            {
                Message = message,
                StatusCode = statusCode,
            };
        };

        return await Handler<string>.Exception(callback);
    }

    [HttpDelete("{id}")]
    public async Task<ApiResponse<string>> Delete(string id)
    {
        AsyncCallback<string> callback = async () => {
            bool isDeleted = await service.DeleteById(id);
            string message = isDeleted ? "Category deleted successfully" : "Cannot delete selected category";
            HttpStatusCode statusCode = isDeleted ? HttpStatusCode.OK : HttpStatusCode.NotModified;

            return new ApiResponse<string>()
            {
                Message = message,
                StatusCode = statusCode,
            };
        };

        return await Handler<string>.Exception(callback);
    }
}