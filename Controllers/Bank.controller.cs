using Microsoft.AspNetCore.Mvc;
using Defination;
using System.Net;
using Services;

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

    [HttpPost]
    public async Task<ApiResponse<string>> Add([FromBody] Bank body)
    {
        AsyncCallback<string> callback = async () => {
            Bank bank = body;
            await service.InserOne(bank);

            return new ApiResponse<string>()
            {
                Message = "Bank inserted successfully",
                StatusCode = HttpStatusCode.Created,
            };
        };

        return await Handler<string>.Exception(callback);
    }

    [HttpGet]
    public async Task<ApiResponse<List<Bank>>> GetList()
    {
        AsyncCallback<List<Bank>> callback = async () => {
            List<Bank> list = await service.GetList();

            return new ApiResponse<List<Bank>>()
            {
                Result = list,
                StatusCode = HttpStatusCode.OK,
            };
        };

        return await Handler<List<Bank>>.Exception(callback);
    }

    [HttpGet("{id}")]
    public async Task<ApiResponse<Bank>> SearcById(string id)
    {
        AsyncCallback<Bank> callback = async () => {
            Bank bank = await service.SearchById(id);

            return new ApiResponse<Bank>()
            {
                StatusCode = HttpStatusCode.OK,
                Result = bank
            };
        };

        return await Handler<Bank>.Exception(callback);
    }

    [HttpPatch("{id}")]
    public async Task<ApiResponse<string>> Update(string id, [FromBody] dynamic body)
    {
        AsyncCallback<string> callback = async () => {
            bool isUpdated = await service.UpdateById(id, body);
            HttpStatusCode statusCode = isUpdated ? HttpStatusCode.Created : HttpStatusCode.NotModified;
            string message = isUpdated ? "Bank updated successfully" : "Bank couldn't be updated";

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
            string message = isDeleted ? "Bank deleted successfully" : "Cannot delete selected bank";
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