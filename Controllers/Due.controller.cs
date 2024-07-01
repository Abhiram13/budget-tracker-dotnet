using Defination;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace budget_tracker.Controllers;

[ApiController]
[Route("due")]
public class DueController : ControllerBase
{
    private readonly IDueService service;

    public DueController(IDueService _service)
    {
        service = _service;
    }

    [HttpPost]
    public async Task<ApiResponse<string>> Insert([FromBody] Due due)
    {
        AsyncCallback<string> callback = async () => {
            service.Validate(due);
            await service.InserOne(due);
            return new ApiResponse<string>() {
                Message = "Due inserted successfully",
                StatusCode = System.Net.HttpStatusCode.Created
            };
        };

        return await Handler<string>.Exception(callback);
    }
}