using Microsoft.AspNetCore.Mvc;
using Defination;
using System.Net;

namespace budget_tracker.Controllers;

[ApiController]
[Route("category")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService service;

    public CategoryController(ICategoryService _service)
    {
        service = _service;
    }

    [HttpPost("add")]
    public async Task<ApiResponse<string>> Add([FromBody] Category body)
    {
        try
        {
            Category category = body;
            await service.InserOne(category);
            return new ApiResponse<string>()
            {
                Message = "Category inserted successfully",
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