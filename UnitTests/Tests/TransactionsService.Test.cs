using BudgetTracker.Entities;
using BudgetTracker.Enums;
using BudgetTracker.Exceptions;
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

    #region Transaction entity unit tests

    [Fact]
    public void Transaction_Entity_Valid()
    {
        double amount = 123.45;
        string description = "Random description";
        string fromBank = "from_bank";
        string toBank = "to_bank";
        string categoryId = "category_id";
        TransactionType type = TransactionType.Debit;
        DateOnly date = DateOnly.FromDateTime(DateTime.UtcNow);
        Transaction transaction = Transaction.Create(amount, description, type, fromBank, toBank, categoryId, date);
    }

    #endregion

    #region Insert Transactions unit tests

    [Fact]
    public void Insert_Transaction_Valid()
    {
        // Arrange
        InsertTransactionDto dto = new InsertTransactionDto
        {
            CategoryId = "random",
            Amount = 123.45,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Description = "Random description",
            DueId = null,
            FromBank = "random_from",
            ToBank = null,
            Type = TransactionType.Debit
        };

        _transactionRepository.Setup(t => t.InsertOneAsync(It.IsAny<Transaction>()));
        _categoryRepository.Setup(c => c.CountByIdAsync(It.IsAny<string>())).ReturnsAsync(true);
        _bankRepository.Setup(b => b.CountByIdAsync(It.IsAny<string>())).ReturnsAsync(true);

        // Act
        _transactionService.InsertOneAsync(dto);

        // Assert
        _transactionRepository.Verify(t => t.InsertOneAsync(It.IsAny<Transaction>()), Times.Once());
    }

    [Fact]
    public async Task Insert_Transaction_InValid_Category_Exception_Async()
    {
        // Arrange
        InsertTransactionDto dto = new InsertTransactionDto
        {
            CategoryId = "random",
            Amount = 123.45,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Description = "Random description",
            DueId = null,
            FromBank = "random_from",
            ToBank = null,
            Type = TransactionType.Debit
        };

        _transactionRepository.Setup(t => t.InsertOneAsync(It.IsAny<Transaction>()));
        _categoryRepository.Setup(c => c.CountByIdAsync(It.IsAny<string>())).ReturnsAsync(false);
        _bankRepository.Setup(b => b.CountByIdAsync(It.IsAny<string>())).ReturnsAsync(true);

        // Act
        Exception exception = await Record.ExceptionAsync(async () =>
        {
            await _transactionService.InsertOneAsync(dto).ConfigureAwait(false);
        });

        // Assert
        // 'InsertOnceAsync' method never calls since category 'CountAsync' method will throw error
        _transactionRepository.Verify(t => t.InsertOneAsync(It.IsAny<Transaction>()), Times.Never());
        Assert.NotNull(exception);
        Assert.IsType<BadRequestException>(exception);
        Assert.Equal("Invalid Category id provided", exception.Message);
    }

    [Fact]
    public async Task Insert_Transaction_Null_FromBank_For_Debit_Exception_Async()
    {
        // Arrange
        InsertTransactionDto dto = new InsertTransactionDto
        {
            CategoryId = "random",
            Amount = 123.45,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Description = "Random description",
            DueId = null,
            FromBank = null,
            ToBank = "random_bank",
            Type = TransactionType.Debit
        };

        _transactionRepository.Setup(t => t.InsertOneAsync(It.IsAny<Transaction>()));
        _categoryRepository.Setup(c => c.CountByIdAsync(It.IsAny<string>())).ReturnsAsync(true);
        _bankRepository.Setup(b => b.CountByIdAsync(It.IsAny<string>())).ReturnsAsync(true);

        // Act
        Exception exception = await Record.ExceptionAsync(async () =>
        {
            await _transactionService.InsertOneAsync(dto).ConfigureAwait(false);
        });

        // Assert
        // 'InsertOnceAsync' method never calls since category 'CountAsync' method will throw error
        _transactionRepository.Verify(t => t.InsertOneAsync(It.IsAny<Transaction>()), Times.Never());
        Assert.NotNull(exception);
        Assert.IsType<BadRequestException>(exception);
        Assert.Equal("Invalid From bank id provided for the transaction debit type", exception.Message);
    }

    [Fact]
    public async Task Insert_Transaction_Invalid_FromBank_For_Debit_Exception_Async()
    {
        // Arrange
        InsertTransactionDto dto = new InsertTransactionDto
        {
            CategoryId = "random",
            Amount = 123.45,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Description = "Random description",
            DueId = null,
            FromBank = "random_bank",
            ToBank = null,
            Type = TransactionType.Debit
        };

        _transactionRepository.Setup(t => t.InsertOneAsync(It.IsAny<Transaction>()));
        _categoryRepository.Setup(c => c.CountByIdAsync(It.IsAny<string>())).ReturnsAsync(true);
        _bankRepository.Setup(b => b.CountByIdAsync(It.IsAny<string>())).ReturnsAsync(false);

        // Act
        Exception exception = await Record.ExceptionAsync(async () =>
        {
            await _transactionService.InsertOneAsync(dto).ConfigureAwait(false);
        });

        // Assert
        // 'InsertOnceAsync' method never calls since category 'CountAsync' method will throw error
        _transactionRepository.Verify(t => t.InsertOneAsync(It.IsAny<Transaction>()), Times.Never());
        Assert.NotNull(exception);
        Assert.IsType<BadRequestException>(exception);
        Assert.Equal("Invalid From bank id provided for the transaction debit type", exception.Message);
    }

    [Fact]
    public async Task Insert_Transaction_Null_ToBank_For_Credit_Exception_Async()
    {
        // Arrange
        InsertTransactionDto dto = new InsertTransactionDto
        {
            CategoryId = "random",
            Amount = 123.45,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Description = "Random description",
            DueId = null,
            FromBank = "random",
            ToBank = null,
            Type = TransactionType.Credit
        };

        _transactionRepository.Setup(t => t.InsertOneAsync(It.IsAny<Transaction>()));
        _categoryRepository.Setup(c => c.CountByIdAsync(It.IsAny<string>())).ReturnsAsync(true);
        _bankRepository.Setup(b => b.CountByIdAsync(It.IsAny<string>())).ReturnsAsync(true);

        // Act
        Exception exception = await Record.ExceptionAsync(async () =>
        {
            await _transactionService.InsertOneAsync(dto).ConfigureAwait(false);
        });

        // Assert
        // 'InsertOnceAsync' method never calls since category 'CountAsync' method will throw error
        _transactionRepository.Verify(t => t.InsertOneAsync(It.IsAny<Transaction>()), Times.Never());
        Assert.NotNull(exception);
        Assert.IsType<BadRequestException>(exception);
        Assert.Equal("Invalid To bank id provided for the transaction credit type", exception.Message);
    }

    [Fact]
    public async Task Insert_Transaction_Invalid_ToBank_For_Credit_Exception_Async()
    {
        // Arrange
        InsertTransactionDto dto = new InsertTransactionDto
        {
            CategoryId = "random",
            Amount = 123.45,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Description = "Random description",
            DueId = null,
            FromBank = null,
            ToBank = "random_bank",
            Type = TransactionType.Credit
        };

        _transactionRepository.Setup(t => t.InsertOneAsync(It.IsAny<Transaction>()));
        _categoryRepository.Setup(c => c.CountByIdAsync(It.IsAny<string>())).ReturnsAsync(true);
        _bankRepository.Setup(b => b.CountByIdAsync(It.IsAny<string>())).ReturnsAsync(false);

        // Act
        Exception exception = await Record.ExceptionAsync(async () =>
        {
            await _transactionService.InsertOneAsync(dto).ConfigureAwait(false);
        });

        // Assert
        // 'InsertOnceAsync' method never calls since category 'CountAsync' method will throw error
        _transactionRepository.Verify(t => t.InsertOneAsync(It.IsAny<Transaction>()), Times.Never());
        Assert.NotNull(exception);
        Assert.IsType<BadRequestException>(exception);
        Assert.Equal("Invalid To bank id provided for the transaction credit type", exception.Message);
    }

    [Fact]
    public async Task Insert_Transaction_Null_FromBank_ToBank_Exception_Async()
    {
        // Arrange
        InsertTransactionDto dto = new InsertTransactionDto
        {
            CategoryId = "random",
            Amount = 123.45,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Description = "Random description",
            DueId = null,
            FromBank = null,
            ToBank = null,
            Type = TransactionType.Credit
        };

        _transactionRepository.Setup(t => t.InsertOneAsync(It.IsAny<Transaction>()));
        _categoryRepository.Setup(c => c.CountByIdAsync(It.IsAny<string>())).ReturnsAsync(true);
        _bankRepository.Setup(b => b.CountByIdAsync(It.IsAny<string>())).ReturnsAsync(false);

        // Act
        Exception exception = await Record.ExceptionAsync(async () =>
        {
            await _transactionService.InsertOneAsync(dto).ConfigureAwait(false);
        });

        // Assert
        // 'InsertOnceAsync' method never calls since category 'CountAsync' method will throw error
        _transactionRepository.Verify(t => t.InsertOneAsync(It.IsAny<Transaction>()), Times.Never());
        Assert.NotNull(exception);
        Assert.IsType<BadRequestException>(exception);
        Assert.Equal("Invalid To bank id and From bank id provided.", exception.Message);
    }

    #endregion
}
