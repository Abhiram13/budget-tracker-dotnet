using Microsoft.AspNetCore.Mvc;
using System.Net;
using BudgetTracker.Services;
using BudgetTracker.Defination;
using BudgetTracker.Injectors;

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

    [Route("error/{code}")]
    public IActionResult Error(int code)
    {
        Dictionary<int, string> messages = new()
        {
            { 400, "Invalid data provided" },
            { 401, "Unauthorise" },
            { 403, "The current user is forbidden to access this content" },
            { 404, "Route not found" },
            { 405, "Method not allowed" },
            { 500, "Something went wrong" }
        };

        Console.WriteLine(code);
        
        switch (code)
        {
            case 400: return BadRequest(new ApiResponse<string> { Message = messages[code], StatusCode = HttpStatusCode.BadRequest });
            case 401: return Unauthorized(new ApiResponse<string> { Message = messages[code], StatusCode = HttpStatusCode.Unauthorized });
            case 403: return new ObjectResult(403) {Value = new ApiResponse<string> { Message = messages[code], StatusCode = HttpStatusCode.Forbidden }};
            case 404: return NotFound(new ApiResponse<string> { Message = messages[code], StatusCode = HttpStatusCode.NotFound });
            case 405: return new ObjectResult(405) {Value = new ApiResponse<string> { Message = messages[code], StatusCode = HttpStatusCode.MethodNotAllowed }};
            default: return Ok(new ApiResponse<string> { StatusCode = HttpStatusCode.OK });
        }
    }
}