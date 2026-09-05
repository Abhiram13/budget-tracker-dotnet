using System.Net;
using BudgetTracker.Core.Application.Exceptions;
using BudgetTracker.Core.Application.Services;
using BudgetTracker.Core.Domain.Entities;
using BudgetTracker.Core.Domain.ValueObject;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace BudgetTracker.Api.Controllers;

[ApiController]
public class BankController : ApiBaseController
{
    private readonly BankService _service;
    private readonly IMemoryCache _cache;
    private readonly string _cacheKey = "bank_cache";

    public BankController(BankService service, IMemoryCache cache)
    {
        _service = service;
        _cache = cache;
    }

    [HttpPost]
    public async Task<ApiResponse<string>> AddOneAsync([FromBody] Bank body)
    {
        await _service.AddOneAsync(body);
        return new ApiResponse<string>()
        {
            Message = "Bank inserted successfully",
            StatusCode = HttpStatusCode.Created,
        };
    }

    [HttpGet]
    public async Task<ApiResponse<List<Bank>>> GetListAsync()
    {
        if (!_cache.TryGetValue(_cacheKey, out List<Bank>? banks))
        {
            banks = await _service.ListAsync();
            _cache.Set(_cacheKey, banks);            
        }

        return new ApiResponse<List<Bank>>()
        {
            Result = banks,
            StatusCode = HttpStatusCode.OK,
        };
    }

    [HttpGet("{id}")]
    public async Task<ApiResponse<Bank>> SearchByIdAsync(string id)
    {
        if (string.IsNullOrEmpty(id)) { throw new BadRequestException("Bank id is missing"); }

        Bank bank = await _service.SearchByIdAsync(id);
        return new ApiResponse<Bank>()
        {
            StatusCode = HttpStatusCode.OK,
            Result = bank
        };
    }

    [HttpPatch("{id}")]
    public async Task<ApiResponse<string>> UpdateOneAsync(string id, [FromBody] Bank body)
    {
        bool isUpdated = await _service.UpdateOneAsync(id, body);
        HttpStatusCode statusCode = isUpdated ? HttpStatusCode.Created : HttpStatusCode.NotModified;
        string message = isUpdated ? "Bank updated successfully" : "Bank couldn't be updated";

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
        string message = isDeleted ? "Bank deleted successfully" : "Cannot delete selected bank";
        HttpStatusCode statusCode = isDeleted ? HttpStatusCode.OK : HttpStatusCode.NotModified;

        return new ApiResponse<string>()
        {
            Message = message,
            StatusCode = statusCode,
        };
    }
}