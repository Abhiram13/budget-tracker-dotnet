using Microsoft.AspNetCore.Mvc;
using System.Net;
using Microsoft.Extensions.Caching.Memory;
using BudgetTracker.Interface;
using BudgetTracker.Defination;

namespace BudgetTracker.Controllers;

[ApiController]
public class BankController : ApiBaseController
{
    private readonly IBankService _service;
    private readonly IMemoryCache _cache;
    private readonly string _cacheKey = "bank_cache";

    public BankController(IBankService service, IMemoryCache memoryCache)
    {
        _service = service;
        _cache = memoryCache;
    }

    [HttpPost]
    public async Task<ApiResponse<string>> Add([FromBody] Bank body)
    {
        Bank bank = body;
        await _service.InserOne(bank);

        _cache.Remove(_cacheKey);

        return new ApiResponse<string>()
        {
            Message = "Bank inserted successfully",
            StatusCode = HttpStatusCode.Created,
        };
    }

    [HttpGet]
    public async Task<ApiResponse<List<Bank>>> GetList()
    {
        if (!_cache.TryGetValue(_cacheKey, out List<Bank>? banks))
        {
            banks = await _service.GetList();
            _cache.Set(_cacheKey, banks);
        }

        return new ApiResponse<List<Bank>>()
        {
            Result = banks,
            StatusCode = HttpStatusCode.OK,
        };
    }

    [HttpGet("{id}")]
    public async Task<ApiResponse<Bank>> SearcById(string id)
    {
        Bank bank = await _service.SearchById(id);

        return new ApiResponse<Bank>()
        {
            StatusCode = HttpStatusCode.OK,
            Result = bank
        };
    }

    [HttpPatch("{id}")]
    public async Task<ApiResponse<string>> Update(string id, [FromBody] Bank body)
    {
        bool isUpdated = await _service.UpdateById(id, body);
        HttpStatusCode statusCode = isUpdated ? HttpStatusCode.Created : HttpStatusCode.NotModified;
        string message = isUpdated ? "Bank updated successfully" : "Bank couldn't be updated";

        _cache.Remove(_cacheKey);

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
        string message = isDeleted ? "Bank deleted successfully" : "Cannot delete selected bank";
        HttpStatusCode statusCode = isDeleted ? HttpStatusCode.OK : HttpStatusCode.NotModified;

        _cache.Remove(_cacheKey);

        return new ApiResponse<string>()
        {
            Message = message,
            StatusCode = statusCode,
        };
    }
}