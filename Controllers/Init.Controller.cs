using Microsoft.AspNetCore.Mvc;
using Defination;
using System.Net;
using Services;

namespace budget_tracker.Controllers;

[ApiController]
[Route("")]
public class InitController : ControllerBase
{
    private readonly IUserService service;

    public InitController(IUserService _service)
    {
        service = _service;
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

        return await Handler<string>.Exception(callback);
    }
}