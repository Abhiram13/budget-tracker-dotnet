using System.Net;
using Abhiram.Secrets.Providers.Interface;
using BudgetTracker.Core.Domain.ValueObject;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTracker.Api.Controllers;

[ApiController]
[Route("")]
public class InitController : ControllerBase
{
    private readonly ILogger<InitController> _logger;
    private readonly ISecretManager _secret;

    public InitController(ILogger<InitController> logger, ISecretManager secretManager)
    {
        _logger = logger;
        _secret = secretManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync()
    {
        _logger.LogInformation("Welcome to Budget Tracker Application :)");
        string? val = await _secret.GetSecretAsync("API-KEY");

        return Ok(new ApiResponse<string>()
        {
            StatusCode = HttpStatusCode.OK,
            Message = $"Welcome to your new budgeting journey! This is {val}"
        });
    }
}