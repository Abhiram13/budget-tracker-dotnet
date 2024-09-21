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
using BudgetTracker.Application;
using BudgetTracker.API.Transactions.ByDate;

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
        _controller = new (_transactionService.Object, _logger!);
    }
}