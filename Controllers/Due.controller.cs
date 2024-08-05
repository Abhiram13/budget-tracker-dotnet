using Microsoft.AspNetCore.Mvc;
using BudgetTracker.Injectors;
using BudgetTracker.Defination;
using BudgetTracker.Services;

namespace BudgetTracker.Controllers;

[ApiController]
public class DueController : ApiBaseController
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