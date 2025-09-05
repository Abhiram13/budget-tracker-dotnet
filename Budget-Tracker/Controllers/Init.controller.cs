using Microsoft.AspNetCore.Mvc;
using System.Net;
// using BudgetTracker.Services;
using BudgetTracker.Defination;
// using BudgetTracker.Interface;
// using Microsoft.AspNetCore.Authorization;
// using BudgetTracker.Application;
using CustomUtilities;
using Microsoft.Extensions.Logging;
using BudgetTracker.Application;

namespace BudgetTracker.Controllers;

public class Order
{
    public required string OrderId { get; set; }
    public decimal TotalOrders { get; set; }
}

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
        // new { Name = "Alice", Age = 30, message = "Welcome to Cloud Logging. This is from Custom package :)" }
        _logger.LogError("Budget Tracker custom Logger: Order {@Order}", new Order { OrderId = "123", TotalOrders = 1267 });
        return Ok(new ApiResponse<string>()
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Welcome to your new budgeting journey! Get started by adding your first transaction and taking control of your finances"
        });
    }
}