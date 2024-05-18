using Microsoft.AspNetCore.Mvc;
using Defination;
using System.Net;

namespace budget_tracker.Controllers;

[ApiController]
[Route("bank")]
public class BankController : ControllerBase
{
    private readonly IBankService service;

    public BankController(IBankService _service)
    {
        service = _service;
    }

    [HttpPost("add")]
    public async Task<ApiResponse<string>> Add([FromBody] Bank body)
    {
        try
        {
            Bank bank = body;
            await service.InserOne(bank);
            return new ApiResponse<string>()
            {
                Message = "Bank inserted successfully",
                StatusCode = HttpStatusCode.Created,
            };
        }
        catch (Exception e)
        {
            return new ApiResponse<string>()
            {
                Message = $"Something went wrong. Message {e.Message}",
                StatusCode = HttpStatusCode.InternalServerError,
            };
        }
    }
}