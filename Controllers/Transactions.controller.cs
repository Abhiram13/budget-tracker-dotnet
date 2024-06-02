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
        DateTime ChangeDate()
        {            
            string date = body.Date.ToString("yyyy-MM-dd");
            string[] split = date.Split("-");
            int year = int.Parse(split[0]);
            int month = int.Parse(split[1]);
            int day = int.Parse(split[2]);

            return new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
        }

        AsyncCallback<string> callback = async () => {
            Transaction transaction = body;
            transaction.Date = ChangeDate();
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
    public async Task<ApiResponse<List<TransactionList>>> Get([FromQuery] string? date)
    {
        AsyncCallback<List<TransactionList>> callback = async () => {
            List<TransactionList> list = await service.ListByDate(date);
            return new ApiResponse<List<TransactionList>>()
            {
                StatusCode = HttpStatusCode.OK,
                Result = list
            };
        };

        return await Handler<List<TransactionList>>.Exception(callback);
    }
}