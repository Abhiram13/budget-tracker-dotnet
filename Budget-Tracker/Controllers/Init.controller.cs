using Microsoft.AspNetCore.Mvc;
using System.Net;
using BudgetTracker.Services;
using BudgetTracker.Defination;
using BudgetTracker.Interface;
using Microsoft.AspNetCore.Authorization;
using CustomUtilities;
// using BudgetTracker.Application;

namespace BudgetTracker.Controllers;

[ApiController]
[Route("")]
public class InitController : ControllerBase
{
    private readonly ILogger<InitController> _logger;

    public InitController(ILogger<InitController> logger)
    {
        _logger = logger;
    }

    [Route("")]
    public IActionResult Get()
    {
        Logger.LogWarning("Hello World! This is from Budget tracker");
        return Ok(new ApiResponse<string>()
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Welcome to your Budget Tracker! This is from Google Compute Engine #2 :)"
        });
    }
}