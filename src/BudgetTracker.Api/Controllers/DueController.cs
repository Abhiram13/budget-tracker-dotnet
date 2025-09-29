using System.Net;
using BudgetTracker.Core.Application.Services;
using BudgetTracker.Core.Domain.Entities;
using BudgetTracker.Core.Domain.Enums;
using BudgetTracker.Core.Domain.ValueObject;
using BudgetTracker.Core.Domain.ValueObject.Dues;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTracker.Api.Controllers;

[ApiController]
public class DuesController : ApiBaseController
{
    private readonly DueService _dueService;

    public DuesController(DueService dueService)
    {
        _dueService = dueService;
    }

    [HttpGet]
    public async Task<ApiResponse<List<DueList>>> ListAsync([FromQuery] DueStatus? status)
    {
        List<DueList> dues = await _dueService.DueListAsync(status);
        return new ApiResponse<List<DueList>>()
        {
            StatusCode = HttpStatusCode.OK,
            Result = dues
        };
    }

    [HttpGet("{id}")]
    public async Task<ApiResponse<DueDetails>> GetByIdAsync(string id)
    {
        DueDetails due = await _dueService.SearchByIdAsync(id);

        return new ApiResponse<DueDetails>
        {
            StatusCode = HttpStatusCode.OK,
            Result = due
        };
    }

    [HttpGet("{id}/transactions")]
    public async Task<ApiResponse<List<DueTransactions>>> GetDueTransactionsByIdAsync(string id)
    {
        List<DueTransactions> dueTransactions = await _dueService.GetDueTransactionsAsync(id);

        return new ApiResponse<List<DueTransactions>>
        {
            StatusCode = HttpStatusCode.OK,
            Result = dueTransactions,
        };
    }

    [HttpPost]
    public async Task<ApiResponse<InsertResultId>> AddOneAsync([FromBody] Due due)
    {
        Due result = await _dueService.AddOneAsync(due);
        Response.StatusCode = StatusCodes.Status201Created;
        return new ApiResponse<InsertResultId>
        {
            StatusCode = HttpStatusCode.Created,
            Message = "Due created successfully",
            Result = new InsertResultId
            {
                Id = result.Id
            }
        };
    }
}