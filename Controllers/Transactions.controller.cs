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

        return await Handler<string>.Exception(callback);
    }

    [HttpGet]
    public async Task<ApiResponse<TransactionListResult>> Get([FromQuery] string? date, [FromQuery] string? month, [FromQuery] string? year)
    {
        _logger.LogError("Sample Second logger for Error");
        _logger.LogCritical("Sample Second logger for Critical");
        _logger.LogWarning("Sample Second logger for Warning");
        _logger.LogDebug("Sample Second logger for Debug");
        _logger.LogInformation("Sample Second logger for Information");
        _logger.LogTrace("Sample Second logger for Trace");
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

        return await Handler<TransactionListResult>.Exception(callback);
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

        return await Handler<TransactionDetails>.Exception(callback);
    }
}