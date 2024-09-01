using Xunit;
using Moq;
using BudgetTracker.Defination;
using BudgetTracker.Controllers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using BudgetTracker.Injectors;
using System.Text.Json;
using System.Reflection;
using BudgetTracker.API.Transactions.List;

namespace UnitTests;

public class TransactionServiceUnitTest
{
    private readonly Mock<ITransactionService> _transactionService;
    private readonly TransactionsController _controller;
    private readonly IMemoryCache _cache;
    private readonly ILogger<TransactionsController>? _logger;

    public TransactionServiceUnitTest()
    {
        _logger = null;
        _cache = new MemoryCache(new MemoryCacheOptions());
        _transactionService = new Mock<ITransactionService>();
        _controller = new TransactionsController(_transactionService.Object, _logger!);
    }

    [Fact]
    public async Task List()
    {
        Result transactionResults = new () {
            TotalCount = 105,
            Transactions = new List<TransactionDetails>() {
                new () { Count = 2, Credit = 97302, Date = "2024-08-10", Debit = 56 },
            },
        };

        QueryParams query = new () { Month = "08", Type = "transaction", Year = "2024" };
        _transactionService.Setup(t => t.List(query)).ReturnsAsync(transactionResults);
        ApiResponse<Result> response = await _controller.Get("08", "2024", "transaction");

        Console.WriteLine(
            JsonSerializer.Serialize(response)
        );

        Assert.Multiple(() => {
            Assert.Equal(200, (int)response.StatusCode);
            Assert.Null(response.GetType().GetProperty("message"));
            Assert.NotNull(response.GetType().GetProperty("result"));
            Assert.True(response.Result?.TotalCount > 0);
            Assert.NotNull(response.GetType().GetProperty("result")?.GetType()?.GetProperty("transactions"));
            Assert.True(response.Result.Transactions?.Count > 0);
        });
    }
}