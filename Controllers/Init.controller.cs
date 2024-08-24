using Microsoft.AspNetCore.Mvc;
using System.Net;
using BudgetTracker.Services;
using BudgetTracker.Defination;
using BudgetTracker.Injectors;
using Microsoft.AspNetCore.Authorization;

namespace BudgetTracker.Controllers;

[ApiController]
[Route("")]
public class InitController : ControllerBase
{
    private readonly IUserService service;
    private readonly ILogger<InitController> _logger;

    public InitController(IUserService _service, ILogger<InitController> logger)
    {
        service = _service;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ApiResponse<string>> Login([FromBody] Login body)
    {
        AsyncCallback<string> callback = async () => {
            string? login = await service.Login(body.Username, body.Password);
            bool isValidLogin = !string.IsNullOrEmpty(login);
            string message = isValidLogin ? "Login successful" : "Invalid credentials provided";
            HttpStatusCode status = isValidLogin ? HttpStatusCode.OK : HttpStatusCode.Unauthorized;            
            HttpContext.Response.StatusCode = (int) status;

            return new ApiResponse<string>()
            {
                Message = message,
                StatusCode = status,
                Result = login
            };
        };

        return await Handler<string>.Exception(callback, _logger);
    }
}