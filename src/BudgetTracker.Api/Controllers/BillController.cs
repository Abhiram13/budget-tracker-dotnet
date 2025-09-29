using BudgetTracker.Core.Application.Services;
using BudgetTracker.Core.Domain.Entities;
using BudgetTracker.Core.Domain.ValueObject;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTracker.Api.Controllers;

[ApiController]
public class BillsController : ApiBaseController
{
    private readonly BillService _billService;

    public BillsController(BillService billService)
    {
        _billService = billService;
    }

    [HttpPost]
    public async Task<ApiResponse<string>> AddOneAsync(Bill body)
    {
        await _billService.AddOneAsync(body);
        return new ApiResponse<string>
        {
            StatusCode = System.Net.HttpStatusCode.Created,
            Message = "Bill created successfully",
        };
    }
}