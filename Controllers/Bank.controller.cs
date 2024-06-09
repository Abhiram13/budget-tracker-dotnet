using Microsoft.AspNetCore.Mvc;
using Defination;
using System.Net;
using Services;
using Microsoft.Extensions.Caching.Memory;

namespace budget_tracker.Controllers;

[ApiController]
[Route("bank")]
public class BankController : ControllerBase
{
    private readonly IBankService service;
    private readonly IMemoryCache cache;
    private readonly string cacheKey = "bank_cache";

    public BankController(IBankService _service, IMemoryCache memoryCache)
    {
        service = _service;
        cache = memoryCache;
    }

    [HttpPost]
    public async Task<ApiResponse<string>> Add([FromBody] Bank body)
    {
        AsyncCallback<string> callback = async () => {
            Bank bank = body;
            await service.InserOne(bank);

            cache.Remove(cacheKey);

            return new ApiResponse<string>()
            {
                Message = "Bank inserted successfully",
                StatusCode = HttpStatusCode.Created,
            };
        };

        return await Handler<string>.Exception(callback);
    }

    [HttpGet]
    public async Task<ApiResponse<List<Bank>>> GetList()
    {
        AsyncCallback<List<Bank>> callback = async () => {
            if (!cache.TryGetValue(cacheKey, out List<Bank> banks))
            {
                banks = await service.GetList();
                cache.Set(cacheKey, banks);
            }

            return new ApiResponse<List<Bank>>()
            {
                Result = banks,
                StatusCode = HttpStatusCode.OK,
            };
        };

        return await Handler<List<Bank>>.Exception(callback);
    }

    [HttpGet("{id}")]
    public async Task<ApiResponse<Bank>> SearcById(string id)
    {
        AsyncCallback<Bank> callback = async () => {
            Bank bank = await service.SearchById(id);

            return new ApiResponse<Bank>()
            {
                StatusCode = HttpStatusCode.OK,
                Result = bank
            };
        };

        return await Handler<Bank>.Exception(callback);
    }

    [HttpPatch("{id}")]
    public async Task<ApiResponse<string>> Update(string id, [FromBody] dynamic body)
    {
        AsyncCallback<string> callback = async () => {
            bool isUpdated = await service.UpdateById(id, body);
            HttpStatusCode statusCode = isUpdated ? HttpStatusCode.Created : HttpStatusCode.NotModified;
            string message = isUpdated ? "Bank updated successfully" : "Bank couldn't be updated";

            cache.Remove(cacheKey);

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
            string message = isDeleted ? "Bank deleted successfully" : "Cannot delete selected bank";
            HttpStatusCode statusCode = isDeleted ? HttpStatusCode.OK : HttpStatusCode.NotModified;

            cache.Remove(cacheKey);

            return new ApiResponse<string>()
            {
                Message = message,
                StatusCode = statusCode,
            };
        };

        return await Handler<string>.Exception(callback);
    }
}