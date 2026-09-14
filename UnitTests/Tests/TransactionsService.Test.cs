using BudgetTracker.Entities;
using BudgetTracker.Enums;
using BudgetTracker.Interfaces;
using BudgetTracker.Models;
using BudgetTracker.Services;
using Moq;
using Xunit;

namespace UnitTests.Tests;

public class TransactionServiceTests
{
    private Mock<ITransactionRepository> _transactionRepository = new Mock<ITransactionRepository>();
    private Mock<ICategoryRepository> _categoryRepository = new Mock<ICategoryRepository>();
    private Mock<IBankRepository> _bankRepository = new Mock<IBankRepository>();
    private TransactionService _transactionService;

    public TransactionServiceTests()
    {
        _transactionService = new TransactionService(_transactionRepository.Object, _categoryRepository.Object, _bankRepository.Object);
    }

    [Fact]
    public async Task Insert_Transaction_Valid_Async()
    {
        InsertTransactionDto dto = new InsertTransactionDto
        {
            CategoryId = "random",
            Amount = 123.45,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Description = "Randome description",
            DueId = null,
            FromBank = "random_from",
            ToBank = null,
            Type = TransactionType.Debit
        };

        _transactionRepository.Setup(t => t.InsertOneAsync(It.IsAny<Transaction>()));

        _transactionService.InsertOneAsync(dto);
        
        _transactionRepository.Verify(t => t.InsertOneAsync(It.IsAny<Transaction>()), Times.Once());
    }
}