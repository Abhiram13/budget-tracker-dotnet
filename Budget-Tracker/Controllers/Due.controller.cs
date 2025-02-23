using System.Net;
using BudgetTracker.API.Dues;
using BudgetTracker.Defination;
using BudgetTracker.Interface;
using BudgetTracker.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace BudgetTracker.Controllers;

[Route("dues")]
[ApiController]
public class DueController : ApiBaseController
{
    private readonly IDues _service;
    private readonly ILogger<DueController> _logger;

    public DueController(IDues service, ILogger<DueController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ApiResponse<string>> InsertAsync([FromBody] Due body)
    {
        async Task<ApiResponse<string>> Callback()
        {
            await _service.InsertOneAsync(body);
            return new ApiResponse<string>()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Message = "Due created successfully",
            };
        }

        return await Handler<string>.Exception(Callback, _logger);
    }

    [HttpGet]
    public async Task<ApiResponse<List<Due>>> ListAsync()
    {
        async Task<ApiResponse<List<Due>>> Callback()
        {
            ProjectionDefinition<Due> projection = Builders<Due>.Projection.Exclude(d => d.Payee);
            List<Due> dues = await _service.GetList(projection);
            return new ApiResponse<List<Due>>()
            {
                Result = dues,
                StatusCode = System.Net.HttpStatusCode.OK,
            };
        };

        return await Handler<List<Due>>.Exception(Callback, _logger);
    }

    [HttpGet("{id}/transactions")]
    public async Task<ApiResponse<DueTransactions>> TransactionsByDueIdAsync(string id)
    {
        async Task<ApiResponse<DueTransactions>> Callback()
        {
            DueTransactions result = await _service.GetDueTransactionsAsync(id);
            return new ApiResponse<DueTransactions>()
            {
                Result = result,
                StatusCode = System.Net.HttpStatusCode.OK,
            };
        }

        return await Handler<DueTransactions>.Exception(Callback, _logger);
    }

    [HttpGet("{id}")]
    public async Task<ApiResponse<Due>> GetByIdAsync(string id)
    {
        async Task<ApiResponse<Due>> Callback()
        {
            Due result = await _service.SearchById(id);
            return new ApiResponse<Due>()
            {
                Result = result,
                StatusCode = System.Net.HttpStatusCode.OK,
            };
        };

        return await Handler<Due>.Exception(Callback, _logger);
    }

    [HttpPatch("{id}")]
    public async Task<ApiResponse<string>> UpdateByIdAsync(string id, [FromBody] Due due)
    {
        async Task<ApiResponse<string>> Callback()
        {
            bool result = await _service.UpdateById(id, due);
            HttpStatusCode statusCode = result ? HttpStatusCode.Created : HttpStatusCode.NotModified;
            string message = result ? "Due updated successfully" : "Due couldn't be updated";
            return new ApiResponse<string>() { Message = message, StatusCode = statusCode};
        };

        return await Handler<string>.Exception(Callback, _logger);
    }
}
