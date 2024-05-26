using Microsoft.AspNetCore.Mvc;
using Defination;
using System.Net;
using Services;

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

    [HttpPost()]
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

    [HttpGet("{id}")]
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

    [HttpGet()]
    public async Task<ApiResponse<List<Category>>> GetList()
    {
        List<Category> list = await service.GetList();

        return new ApiResponse<List<Category>>()
        {
            Result = list,
            StatusCode = HttpStatusCode.OK,
        };
    }

    [HttpPatch("{id}")]
    public async Task<ApiResponse<string>> Update(string id, [FromBody] dynamic body)
    {
        AsyncCallback<string> callback = async () => {
            bool isUpdated = await service.UpdateById(id, body);
            HttpStatusCode statusCode = isUpdated ? HttpStatusCode.Created : HttpStatusCode.NotModified;
            string message = isUpdated ? "Category updated successfully" : "Category couldn't be updated";

            return new ApiResponse<string>()
            {
                Message = message,
                StatusCode = statusCode,
            };
        };

        return await Handler<string>.Exception(callback);
    }

    [HttpDelete("{id}")]
    public async Task<ApiResponse<string>> Delete(string id)
    {
        AsyncCallback<string> callback = async () => {
            bool isDeleted = await service.DeleteById(id);
            string message = isDeleted ? "Category deleted successfully" : "Cannot delete selected category";
            HttpStatusCode statusCode = isDeleted ? HttpStatusCode.OK : HttpStatusCode.NotModified;

            return new ApiResponse<string>()
            {
                Message = message,
                StatusCode = statusCode,
            };
        };

        return await Handler<string>.Exception(callback);
    }
}