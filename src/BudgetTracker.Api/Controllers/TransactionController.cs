using System.Net;
using BudgetTracker.Core.Application.Services;
using BudgetTracker.Core.Domain.Entities;
using BudgetTracker.Core.Domain.ValueObject;
using BudgetTracker.Core.Domain.ValueObject.Transaction.List;
using Microsoft.AspNetCore.Mvc;

using ListResult = BudgetTracker.Core.Domain.ValueObject.Transaction.List.Result;
using BankResult = BudgetTracker.Core.Domain.ValueObject.Transaction.ByBank.ResultByBank;
using CategoryResult = BudgetTracker.Core.Domain.ValueObject.Transaction.ByCategory.Result;
using BudgetTracker.Core.Domain.ValueObject.Transaction;

namespace BudgetTracker.Api.Controllers;

[ApiController]
public class TransactionsController : ApiBaseController
{
    private readonly TransactionService _service;

    public TransactionsController(TransactionService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ApiResponse<ListResult>> GetAsync(
        [FromQuery] string? month,
        [FromQuery] string? year,
        [FromQuery] string? type,
        [FromQuery] string? sort,
        CancellationToken ct
    ) {
        QueryParams queryParams = new QueryParams()
        {
            Month = month,
            Year = year,
            Type = type,
            SortOrder = sort
        };

        ListResult list = await _service.ListAsync(queryParams, ct);
        return new ApiResponse<ListResult>()
        {
            StatusCode = HttpStatusCode.OK,
            Result = list
        };
    }

    [HttpGet("date/{date}")]
    public async Task<ApiResponse<ByDateTransactions>> GetAsync(string date)
    {
        ByDateTransactions data = await _service.ListByDateAsync(date);

        return new ApiResponse<ByDateTransactions>()
        {
            StatusCode = HttpStatusCode.OK,
            Result = data
        };
    }

    [HttpGet("{id}")]
    public async Task<ApiResponse<Transaction>> GetByIdAsync(string id)
    {
        Transaction transaction = await _service.GetByIdAsync(id);

        return new ApiResponse<Transaction>()
        {
            StatusCode = HttpStatusCode.OK,
            Result = transaction
        };
    }

    [HttpGet("category/{categoryId}")]
    public async Task<ApiResponse<CategoryResult>> GetByCategoryAsync(string categoryId, [FromQuery] string? month, [FromQuery] string? year)
    {
        CategoryResult result = await _service.ListByCategoryAsync(categoryId, new QueryParams()
        {
            Month = month,
            Year = year
        });
        return new ApiResponse<CategoryResult>()
        {
            StatusCode = HttpStatusCode.OK,
            Result = result,
        };
    }

    [HttpGet("bank/{bankId}")]
    public async Task<ApiResponse<BankResult>> GetByBankIdAsync(string bankId, [FromQuery] string? month, [FromQuery] string? year)
    {
        BankResult result = await _service.ListByBankAsync(bankId, new QueryParams()
        {
            Month = month,
            Year = year
        });
        return new ApiResponse<BankResult>()
        {
            StatusCode = HttpStatusCode.OK,
            Result = result,
        };
    }

    [HttpPost]
    public async Task<ApiResponse<string>> AddAsync([FromBody] Transaction body)
    {
        await _service.InsertOneAsync(body);
        return new ApiResponse<string>()
        {
            Message = "Transaction inserted successfully",
            StatusCode = HttpStatusCode.Created,
        };
    }

    [HttpPatch("{id}")]
    public async Task<ApiResponse<string>> UpdateOneAsync(string id, [FromBody] Transaction body)
    {
        bool isUpdated = await _service.UpdateOnAsync(id, body);
        HttpStatusCode statusCode = isUpdated ? HttpStatusCode.Created : HttpStatusCode.NotModified;
        string message = isUpdated ? "Transaction updated successfully" : "Transaction couldn't be updated";

        return new ApiResponse<string>()
        {
            Message = message,
            StatusCode = statusCode,
        };
    }

    [HttpDelete("{id}")]
    public async Task<ApiResponse<string>> DeleteOneAsync(string id)
    {
        bool isDeleted = await _service.DeleteOneAsync(id);
        HttpStatusCode statusCode = isDeleted ? HttpStatusCode.OK : HttpStatusCode.NotModified;
        string message = isDeleted ? "Transaction deleted successfully" : "Transaction couldn't be deleted";

        return new ApiResponse<string>()
        {
            Message = message,
            StatusCode = statusCode,
        };
    }
}