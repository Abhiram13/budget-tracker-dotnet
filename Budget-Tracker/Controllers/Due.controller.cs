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
        await _service.InsertOneAsync(body);
        return new ApiResponse<string>()
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Due created successfully",
        };
    }

    [HttpGet]
    public async Task<ApiResponse<List<Due>>> ListAsync()
    {
        ProjectionDefinition<Due> projection = Builders<Due>.Projection.Exclude(d => d.Payee);
        List<Due> dues = await _service.GetList(projection);
        return new ApiResponse<List<Due>>()
        {
            Result = dues,
            StatusCode = HttpStatusCode.OK,
        };
    }

    [HttpGet("{id}/transactions")]
    public async Task<ApiResponse<DueTransactions>> DueTransactionsAsync(string id)
    {
        DueTransactions result = await _service.GetDueTransactionsAsync(id);
        return new ApiResponse<DueTransactions>()
        {
            Result = result,
            StatusCode = HttpStatusCode.OK,
        };
    }

    [HttpGet("{id}")]
    public async Task<ApiResponse<Due>> GetByIdAsync(string id)
    {
        Due result = await _service.SearchById(id);
        return new ApiResponse<Due>()
        {
            Result = result,
            StatusCode = HttpStatusCode.OK,
        };
    }

    [HttpPatch("{id}")]
    public async Task<ApiResponse<string>> UpdateByIdAsync(string id, [FromBody] Due due)
    {
        bool result = await _service.UpdateById(id, due);
        HttpStatusCode statusCode = result ? HttpStatusCode.Created : HttpStatusCode.NotModified;
        string message = result ? "Due updated successfully" : "Due couldn't be updated";
        return new ApiResponse<string>() { Message = message, StatusCode = statusCode};
    }
}
