using Microsoft.AspNetCore.Mvc;
using System.Net;
using BudgetTracker.Services;
using BudgetTracker.Defination;
using BudgetTracker.Injectors;

// Alias type - similar to Typescript's Type keyword
using TransactionListResult = API.Transactions.List.Result;
using TransactionDetails = API.Transactions.ByDate.Detail;

namespace BudgetTracker.Controllers;

[ApiController]
public class TransactionsController : ApiBaseController
{
    private readonly ITransactionService _service;
    private readonly ILogger _logger;

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
            await _service.Validations(body);
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
    public async Task<ApiResponse<TransactionListResult>> Get([FromQuery] string? date, [FromQuery] string? month, [FromQuery] string? year)
    {
        AsyncCallback<TransactionListResult> callback = async () => {
            API.Transactions.List.QueryParams queryParams = new API.Transactions.List.QueryParams() {
                date = date,
                month = month,
                year = year
            };

            TransactionListResult list = await _service.List(queryParams);
            return new ApiResponse<TransactionListResult>()
            {
                StatusCode = HttpStatusCode.OK,
                Result = list
            };
        };

        return await Handler<TransactionListResult>.Exception(callback, _logger);
    }

    [HttpGet("{date}")]
    public async Task<ApiResponse<TransactionDetails>> Get(string date)
    {
        AsyncCallback<TransactionDetails> callback = async () => {
            TransactionDetails data = await _service.ListByDate(date);

            return new ApiResponse<TransactionDetails>()
            {
                StatusCode = HttpStatusCode.OK,
                Result = data
            };
        };

        return await Handler<TransactionDetails>.Exception(callback, _logger);
    }
}