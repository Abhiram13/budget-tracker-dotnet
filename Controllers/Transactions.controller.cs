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
    public async Task<ApiResponse<List<TransactionList<string>>>> Get([FromQuery] string? date)
    {
        AsyncCallback<List<TransactionList<string>>> callback = async () => {
            List<TransactionList<string>> list = await service.ListByDate(date);
            return new ApiResponse<List<TransactionList<string>>>()
            {
                StatusCode = HttpStatusCode.OK,
                Result = list
            };
        };

        return await Handler<List<TransactionList<string>>>.Exception(callback);
    }
}