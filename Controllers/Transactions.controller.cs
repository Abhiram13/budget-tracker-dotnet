using Microsoft.AspNetCore.Mvc;
using Defination;
using System.Net;
using Services;

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

    [HttpPost("add")]
    public async Task<ApiResponse<string>> Add([FromBody] Transaction body)
    {
        Func<Task<ApiResponse<string>>> callback = async () => {
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

    [HttpGet("list")]
    public ApiResponse<List<TransactionList>> Get()
    {
        Func<ApiResponse<List<TransactionList>>> callback = () => {
            List<TransactionList> list = service.List();
            return new ApiResponse<List<TransactionList>>()
            {
                StatusCode = HttpStatusCode.OK,
                Result = list,
            };
        };

        return Handler<List<TransactionList>>.Exception(callback);
    }
}