using System.Net;
using BudgetTracker.Core.Domain.ValueObject;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTracker.Api.Controllers;

[ApiController]
public class DuesController : ApiBaseController
{
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