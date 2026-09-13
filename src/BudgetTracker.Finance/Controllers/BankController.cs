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
public class BanksController : ApiBaseController
{
    private readonly BankService _service;
    private readonly IMemoryCache _cache;
    private readonly string _cacheKey = "bank_cache";

    public BanksController(BankService service, IMemoryCache cache)
    {
        _service = service;
        _cache = cache;
    }

    [HttpPost]
    public async Task<IActionResult> AddOneAsync([FromBody, Required] InsertBankDto body)
    {
        await _service.AddOneAsync(body.Name);
        return StatusCode(StatusCodes.Status201Created, new ApiResponse { Message = "Bank inserted successfully", StatusCode = HttpStatusCode.Created });
    }

    [HttpGet]
    public async Task<ValueObject.ApiResponse<List<Bank>>> GetListAsync()
    {
        if (!_cache.TryGetValue(_cacheKey, out List<Bank>? banks))
        {
            banks = await _service.ListAsync();
            _cache.Set(_cacheKey, banks);            
        }

        return new ValueObject.ApiResponse<List<Bank>>()
        {
            Result = banks,
            StatusCode = HttpStatusCode.OK,
        };
    }

    [HttpGet("{id}")]
    public async Task<ValueObject.ApiResponse<Bank>> SearchByIdAsync(string id)
    {
        if (string.IsNullOrEmpty(id)) { throw new BadRequestException("Bank id is missing"); }

        Bank bank = await _service.SearchByIdAsync(id);
        return new ValueObject.ApiResponse<Bank>()
        {
            StatusCode = HttpStatusCode.OK,
            Result = bank
        };
    }

    [HttpPatch("{id}")]
    public async Task<ValueObject.ApiResponse<string>> UpdateOneAsync(string id, [FromBody] Bank body)
    {
        bool isUpdated = await _service.UpdateOneAsync(id, body);
        HttpStatusCode statusCode = isUpdated ? HttpStatusCode.Created : HttpStatusCode.NotModified;
        string message = isUpdated ? "Bank updated successfully" : "Bank couldn't be updated";

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
        string message = isDeleted ? "Bank deleted successfully" : "Cannot delete selected bank";
        HttpStatusCode statusCode = isDeleted ? HttpStatusCode.OK : HttpStatusCode.NotModified;

        return new ValueObject.ApiResponse<string>()
        {
            Message = message,
            StatusCode = statusCode,
        };
    }
}