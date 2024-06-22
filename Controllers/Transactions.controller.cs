using Microsoft.AspNetCore.Mvc;
using Defination;
using System.Net;
using Services;
using Global;

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
    public async Task<ApiResponse<List<TransactionList<string>>>> Get([FromQuery] string? date, [FromQuery] string? month, [FromQuery] string? year)
    {
        AsyncCallback<List<TransactionList<string>>> callback = async () => {
            Defination.TransactionsList.QueryParams queryParams = new Defination.TransactionsList.QueryParams() {
                date = date,
                month = month,
                year = year
            };

            List<TransactionList<string>> list = await service.List(queryParams);
            return new ApiResponse<List<TransactionList<string>>>()
            {
                StatusCode = HttpStatusCode.OK,
                Result = list
            };
        };

        return await Handler<List<TransactionList<string>>>.Exception(callback);
    }

    [HttpGet("{date}")]
    public async Task<ApiResponse<TransactionsByDate.Result>> Get(string date)
    {
        AsyncCallback<TransactionsByDate.Result> callback = async () => {
            TransactionsByDate.Result data = await service.ListByDate(date);

            return new ApiResponse<TransactionsByDate.Result>()
            {
                StatusCode = HttpStatusCode.OK,
                Result = data
            };
        };

        return await Handler<TransactionsByDate.Result>.Exception(callback);
    }
}