using System.Net;
using BudgetTracker.Core.Application.Services;
using BudgetTracker.Core.Domain.ValueObject;
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
    public async Task<ApiResponse<string>> ListAsync([FromQuery] string? filter)
    {
        // List<DueList> dues = await _service.ListAsync(filter);
        return new ApiResponse<string>()
        {
            StatusCode = HttpStatusCode.OK,
        };
    }
}