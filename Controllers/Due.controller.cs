using BudgetTracker.API.Dues;
using BudgetTracker.Defination;
using BudgetTracker.Injectors;
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
    public async Task<ApiResponse<DueTransactions>> GetByIdAsync(string id)
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
}
