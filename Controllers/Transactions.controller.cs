using Microsoft.AspNetCore.Mvc;
using System.Net;
using BudgetTracker.Services;
using BudgetTracker.Defination;
using BudgetTracker.Injectors;
using BudgetTracker.API.Transactions.ByDate;

// Alias type - similar to Typescript's Type keyword
using TransactionListResult = BudgetTracker.API.Transactions.List.Result;

namespace BudgetTracker.Controllers;

[ApiController]
public class TransactionsController : ApiBaseController
{
    private readonly ITransactionService _service;
    private readonly ILogger<TransactionsController> _logger;

    public TransactionsController(ITransactionService service, ILogger<TransactionsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ApiResponse<string>> Add([FromBody] Transaction body)
    {
        AsyncCallback<string> callback = async () => {
            Transaction transaction = body;
            // await _service.Validations(body);
            await _service.InserOne(transaction);
            return new ApiResponse<string>()
            {
                Message = "Transaction inserted successfully",
                StatusCode = HttpStatusCode.Created,
            };
        };        

        return await Handler<string>.Exception(callback, _logger);
    }

    [HttpGet]
    public async Task<ApiResponse<TransactionListResult>> Get([FromQuery] string? month, [FromQuery] string? year, [FromQuery] string? type, CancellationToken ct)
    {
        AsyncCallback<TransactionListResult> callback = async () => {
            API.Transactions.List.QueryParams queryParams = new API.Transactions.List.QueryParams() {
                Month = month,
                Year = year,
                Type = type
            };

            TransactionListResult list = await _service.List(queryParams, ct);
            return new ApiResponse<TransactionListResult>()
            {
                StatusCode = HttpStatusCode.OK,
                Result = list
            };
        };

        return await Handler<TransactionListResult>.Exception(callback, _logger);
    }

    [HttpGet("date/{date}")]
    public async Task<ApiResponse<Data>> Get(string date)
    {
        AsyncCallback<Data> callback = async () => {
            Data data = await _service.ListByDate(date);

            return new ApiResponse<Data>()
            {
                StatusCode = HttpStatusCode.OK,
                Result = data
            };
        };

        return await Handler<Data>.Exception(callback, _logger);
    }

    [HttpPatch("{id}")]
    public async Task<ApiResponse<string>> Update(string id, [FromBody] dynamic body)
    {
        AsyncCallback<string> callback = async () => {
            bool isUpdated = await _service.UpdateById(id, body);
            HttpStatusCode statusCode = isUpdated ? HttpStatusCode.Created : HttpStatusCode.NotModified;
            string message = isUpdated ? "Transaction updated successfully" : "Transaction couldn't be updated";

            return new ApiResponse<string>()
            {
                Message = message,
                StatusCode = statusCode,
            };
        };

        return await Handler<string>.Exception(callback, _logger);
    }

    [HttpDelete("{id}")]
    public async Task<ApiResponse<string>> Delete(string id)
    {
        async Task<ApiResponse<string>> Callback()
        {
            bool isDeleted = await _service.DeleteById(id);
            HttpStatusCode statusCode = isDeleted ? HttpStatusCode.OK : HttpStatusCode.NotModified;
            string message = isDeleted ? "Transaction deleted successfully" : "Transaction couldn't be deleted";

            return new ApiResponse<string>()
            {
                Message = message,
                StatusCode = statusCode,
            };
        }
        
        return await Handler<string>.Exception(Callback, _logger);
    }

    [HttpGet("{id}")]
    public async Task<ApiResponse<Transaction>> GetById(string id)
    {
        async Task<ApiResponse<Transaction>> Callback()
        {
            Transaction transaction = await _service.SearchById(id);

            return new ApiResponse<Transaction>()
            {
                StatusCode = HttpStatusCode.OK,
                Result = transaction
            };
        }
        
        return await Handler<Transaction>.Exception(Callback, _logger);
    }
}