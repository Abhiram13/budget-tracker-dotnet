using Defination;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace budget_tracker.Controllers;

[ApiController]
[Route("due")]
public class DueController : ControllerBase
{
    private readonly IDueService _service;
    private readonly ILogger _logger;

    public DueController(IDueService service, ILogger<DueController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ApiResponse<string>> Insert([FromBody] Due due)
    {
        AsyncCallback<string> callback = async () => {
            _service.Validate(due);
            await _service.InserOne(due);
            return new ApiResponse<string>() {
                Message = "Due inserted successfully",
                StatusCode = System.Net.HttpStatusCode.Created
            };
        };

        return await Handler<string>.Exception(callback, _logger);
    }
}