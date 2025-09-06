using Microsoft.AspNetCore.Mvc;
using System.Net;
// using BudgetTracker.Services;
using BudgetTracker.Defination;
// using BudgetTracker.Interface;
// using Microsoft.AspNetCore.Authorization;
// using BudgetTracker.Application;
using CustomUtilities;
using Serilog.Context;

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
        // Logger.LogInformation(
        //     message: "Welcome to your new budgeting journey! Get started by adding your first transaction and taking control of your finances"
        // );
        
        var traceId = Guid.NewGuid().ToString();

        using (LogContext.PushProperty("TraceId", traceId))
        {
            _logger.LogError("Budget Tracker custom Logger: Order {@Order}", new Order { OrderId = "123", TotalOrders = 1267 });
        }        

        return Ok(new ApiResponse<string>()
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Welcome to your new budgeting journey! Get started by adding your first transaction and taking control of your finances"
        });
    }
}