using Microsoft.AspNetCore.Mvc;
using Defination;
using Services;

namespace budget_tracker.Controllers;

[ApiController]
[Route("transactions")]
public class TransactionsController : ControllerBase
{
    [HttpGet]
    public async Task<ApiResponse<string>> Add()
    {
        try
        {
            Transaction transaction = new()
            {
                Amount = 32.5,
                CategoryId = "",
                Date = DateTime.Now,
                Description = "Sample transation",
                Due = false,
                FromBank = "",
                ToBank = "",
                Type = TransactionType.Debit
            };

            await Mongo.DB.GetCollection<Transaction>("transactions").InsertOneAsync(transaction);

            return new ApiResponse<string>()
            {
                Message = "Transaction inserted successfully",
                StatusCode = System.Net.HttpStatusCode.Created,
            };
        }
        catch (Exception e)
        {
            return new ApiResponse<string>()
            {
                Message = $"Something went wrong. Message {e.Message}",
                StatusCode = System.Net.HttpStatusCode.InternalServerError,
            };
        }
    }
}