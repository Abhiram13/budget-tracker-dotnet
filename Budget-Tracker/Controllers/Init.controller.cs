using Microsoft.AspNetCore.Mvc;
using System.Net;
using BudgetTracker.Services;
using BudgetTracker.Defination;
using BudgetTracker.Interface;
using Microsoft.AspNetCore.Authorization;
using BudgetTracker.Application;

namespace BudgetTracker.Controllers;

[ApiController]
[Route("")]
public class InitController : ControllerBase
{
    public InitController() { }

    [Route("")]
    public IActionResult Get()
    {
        Dictionary<string, string> logDetails = new Dictionary<string, string>
        {
            ["Url"] = Request.Path,
            ["HttpMethod"] = Request.Method,
            ["ClientIpAddress"] = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "N/A",
        };

        Logger.LogError(
            exception: new Exception("This is a test base exception"),
            message: "Welcome to your new budgeting journey! Get started by adding your first transaction and taking control of your finances",
            logDetails: logDetails
        );
        return Ok(new ApiResponse<string>()
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Welcome to your new budgeting journey! Get started by adding your first transaction and taking control of your finances"
        });
    }
}