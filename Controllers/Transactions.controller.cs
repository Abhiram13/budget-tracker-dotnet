using Microsoft.AspNetCore.Mvc;
using Defination;
using System.Net;
using Services;

// Alias type - similar to Typescript's Type keyword
using TransactionListResult = API.Transactions.List.Result;
using TransactionDetails = API.Transactions.ByDate.Detail;

namespace budget_tracker.Controllers;

[ApiController]
[Route("transactions")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService service;

    public TransactionsController(ITransactionService _service)
    {
        service = _service;
    }

    [HttpPost]
    public async Task<ApiResponse<string>> Add([FromBody] Transaction body)
    {
        AsyncCallback<string> callback = async () => {
            Transaction transaction = body;
            await service.Validations(body);
            await service.InserOne(transaction);
            return new ApiResponse<string>()
            {
                Message = "Transaction inserted successfully",
                StatusCode = HttpStatusCode.Created,
            };
        };        

        return await Handler<string>.Exception(callback);
    }

    [HttpGet]
    public async Task<ApiResponse<TransactionListResult>> Get([FromQuery] string? date, [FromQuery] string? month, [FromQuery] string? year)
    {
        AsyncCallback<TransactionListResult> callback = async () => {
            API.Transactions.List.QueryParams queryParams = new API.Transactions.List.QueryParams() {
                date = date,
                month = month,
                year = year
            };

            TransactionListResult list = await service.List(queryParams);
            return new ApiResponse<TransactionListResult>()
            {
                StatusCode = HttpStatusCode.OK,
                Result = list
            };
        };

        return await Handler<TransactionListResult>.Exception(callback);
    }

    [HttpGet("{date}")]
    public async Task<ApiResponse<TransactionDetails>> Get(string date)
    {
        AsyncCallback<TransactionDetails> callback = async () => {
            TransactionDetails data = await service.ListByDate(date);

            return new ApiResponse<TransactionDetails>()
            {
                StatusCode = HttpStatusCode.OK,
                Result = data
            };
        };

        return await Handler<TransactionDetails>.Exception(callback);
    }
}