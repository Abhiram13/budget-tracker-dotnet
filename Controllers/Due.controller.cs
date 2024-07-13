using Microsoft.AspNetCore.Mvc;
using BudgetTracker.Injectors;
using BudgetTracker.Defination;
using BudgetTracker.Services;

namespace BudgetTracker.Controllers;

[ApiController]
public class DueController : ApiBaseController
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