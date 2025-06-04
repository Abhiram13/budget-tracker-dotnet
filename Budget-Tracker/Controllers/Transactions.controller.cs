using Microsoft.AspNetCore.Mvc;
using System.Net;
using BudgetTracker.Services;
using BudgetTracker.Defination;
using BudgetTracker.Interface;
using BudgetTracker.API.Transactions.ByDate;
using TransactionListResult = BudgetTracker.API.Transactions.List.Result;
using ByBankResult = BudgetTracker.API.Transactions.ByBank.Result;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Microsoft.AspNetCore.Authorization;

namespace BudgetTracker.Controllers;

[ApiController]
public class TransactionsController : ApiBaseController
{
    private readonly ITransactionService _service;
    private readonly ILogger<TransactionsController> _logger;
    private readonly MongoDatabase _database;

    public TransactionsController(ITransactionService service, ILogger<TransactionsController> logger, MongoDatabase database)
    {
        _service = service;
        _logger = logger;
        _database = database;
    }

    [HttpPost]
    public async Task<ApiResponse<string>> Add([FromBody] Transaction body)
    {
        Transaction transaction = body;
        await _service.Validations(body);
        await _service.InserOne(transaction);
        return new ApiResponse<string>()
        {
            Message = "Transaction inserted successfully",
            StatusCode = HttpStatusCode.Created,
        };
    }

    [HttpGet]
    public async Task<ApiResponse<TransactionListResult>> Get(
        [FromQuery] string? month,
        [FromQuery] string? year,
        [FromQuery] string? type,
        [FromQuery] string? sort,
        CancellationToken ct
    )
    {
        API.Transactions.List.QueryParams queryParams = new API.Transactions.List.QueryParams()
        {
            Month = month,
            Year = year,
            Type = type,
            SortOrder = sort
        };

        TransactionListResult list = await _service.List(queryParams, ct);
        return new ApiResponse<TransactionListResult>()
        {
            StatusCode = HttpStatusCode.OK,
            Result = list
        };
    }

    [HttpGet("date/{date}")]
    public async Task<ApiResponse<Data>> Get(string date)
    {
        Data data = await _service.ListByDate(date);

        return new ApiResponse<Data>()
        {
            StatusCode = HttpStatusCode.OK,
            Result = data
        };
    }

    [HttpPatch("{id}")]
    public async Task<ApiResponse<string>> Update(string id, [FromBody] Transaction body)
    {
        bool isUpdated = await _service.UpdateById(id, body);
        HttpStatusCode statusCode = isUpdated ? HttpStatusCode.Created : HttpStatusCode.NotModified;
        string message = isUpdated ? "Transaction updated successfully" : "Transaction couldn't be updated";

        return new ApiResponse<string>()
        {
            Message = message,
            StatusCode = statusCode,
        };
    }

    [HttpDelete("{id}")]
    public async Task<ApiResponse<string>> Delete(string id)
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

    [HttpGet("{id}")]
    public async Task<ApiResponse<Transaction>> GetById(string id)
    {
        Transaction transaction = await _service.SearchById(id);

        return new ApiResponse<Transaction>()
        {
            StatusCode = HttpStatusCode.OK,
            Result = transaction
        };
    }

    [HttpGet("category/{categoryId}")]
    public async Task<ApiResponse<API.Transactions.ByCategory.Result>> GetByCategory(string categoryId, [FromQuery] string? month, [FromQuery] string? year)
    {
        API.Transactions.ByCategory.Result result = await _service.GetByCategory(categoryId, new API.Transactions.List.QueryParams()
        {
            Month = month,
            Year = year
        });
        return new ApiResponse<API.Transactions.ByCategory.Result>()
        {
            StatusCode = HttpStatusCode.OK,
            Result = result,
        };
    }

    [HttpGet("bank/{bankId}")]
    public async Task<ApiResponse<ByBankResult>> GetByBankId(string bankId, [FromQuery] string? month, [FromQuery] string? year)
    {
        ByBankResult result = await _service.GetByBank(bankId, new API.Transactions.List.QueryParams()
        {
            Month = month,
            Year = year
        });
        return new ApiResponse<ByBankResult>()
        {
            StatusCode = HttpStatusCode.OK,
            Result = result,
        };
    }

    [HttpGet, Route("test"), AllowAnonymous]
    public async Task<List<TransactionTestData>> TestAsync()
    {
        List<TransactionTestData> list = await _database.TransactionCollection.Aggregate()
            .Match(s => s.Date == "2024-09-18")            
            .Group(s => 1, g => new TransactionTestData
            {
                Debit = g.Where(a => a.Type == TransactionType.Debit).Sum(b => b.Amount),
                Credit = g.Where(a => a.Type == TransactionType.Credit).Sum(b => b.Amount),
                TransactionsByDate = g.Select(t => new TransactionTestData.TransactionData
                {
                    Amount = t.Amount,
                    Description = t.Description,                    
                }).ToList()
            })
            .ToListAsync();

        return list;
    }
}