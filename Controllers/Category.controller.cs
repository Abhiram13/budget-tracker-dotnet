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

    [HttpGet("search/{id}")]
    public async Task<ApiResponse<Category>> SearcById(string id)
    {
        try
        {
            Category category = await service.SearchById(id);

            return new ApiResponse<Category>()
            {
                StatusCode = HttpStatusCode.OK,
                Result = category
            };
        }
        catch (Exception e)
        {
            return new ApiResponse<Category>()
            {
                StatusCode = HttpStatusCode.OK,
                Message = $"Something went wrong. Message {e.Message}",
            };
        }
    }

    [HttpGet("list")]
    public async Task<ApiResponse<List<Category>>> GetList()
    {
        List<Category> list = await service.GetList();

        return new ApiResponse<List<Category>>()
        {
            Result = list,
            StatusCode = HttpStatusCode.OK,
        };
    }
}