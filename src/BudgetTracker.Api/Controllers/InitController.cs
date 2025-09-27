using System.Net;
using BudgetTracker.Core.Domain.ValueObject;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTracker.Api.Controllers;

[ApiController]
[Route("")]
public class InitController : ControllerBase
{
    private readonly ILogger<InitController> _logger;

    public InitController(ILogger<InitController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get()
    {
        _logger.LogInformation("Welcome to Budget Tracker Application :)");

        return Ok(new ApiResponse<string>()
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Welcome to your new budgeting journey! Get started by adding your first transaction and taking control of your finances. This is new Domain Arch :)"
        });
    }
}