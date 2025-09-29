using BudgetTracker.Core.Domain.Entities;

using TransactionsList = BudgetTracker.Core.Domain.ValueObject.Transaction.TransactionsList;
using ByBankResults = BudgetTracker.Core.Domain.ValueObject.Transaction.ByBank.ResultByBank;
using ByCategoryData = BudgetTracker.Core.Domain.ValueObject.Transaction.ByCategory.CategoryData;

namespace BudgetTracker.Tests.IntegrationTests.Definations.Transactions
{
    /// <summary>
    /// Test data Object for Transactions by date <c>/transactions/date/:date</c>
    /// </summary>
    public record ByDateTestDef
    (
        string Date,
        int ExpectedHttpStatusCode,
        int ExpectedStatusCode,
        int ExpectedDebit,
        int ExpectedCredit,
        int ExpectedPartialDebit,
        int ExpectedPartialCredit,
        int ExpectedTotalTransactions,
        TransactionsList[] ExpectedTransactions
    );

    /// <summary>
    /// Test data object for Transactions by Id <c>/transactions/:id</c>
    /// </summary>
    public record TransactionByIdTestDef
    (
        bool ShouldIdBeValidTest,
        int ExpectedHttpStatusCode,
        int ExpectedStatusCode,
        string date,
        Transaction Transaction
    );

    /// <summary>
    /// Test data object for Transactions by Bank <br />
    /// <c>/transactions/bank/:bankid</c>
    /// </summary>
    public record ByBankTestDef
    (
        string BankId,
        string Month,
        string Year,
        int ExpectedStatusCode,
        int ExcpectedHttpStatusCode,
        int ExpectedTotalTransactions,
        ByBankResults ExpectedResult
    );

    /// <summary>
    /// Test data object for Transactions list by category type<br/>
    /// <c>/transactions?type="category"</c>
    /// </summary>
    public class TransactionsListByCategoryTypeTestDef
    {
        public string Month { get; set; } = string.Empty;
        public string Year { get; set; } = string.Empty;
        public int ExpectedStatusCode { get; set; }
        public int ExpectedTotalTransactions { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int ExpectedDebit { get; set; }
        public bool ShouldHaveData { get; set; }
    }

    /// <summary>
    /// Test data object for Transactions list by bank type<br/>
    /// <c>/transactions?type="bank"</c>
    /// </summary>
    public record TransactionsListByBankTypeTestDef
    (
        string Month,
        string Year,
        string ExpectedBankId,
        string ExpectedBankName,
        int ExpectedStatusCode,
        int ExpectedTotalTransactions,
        int ExpectedDebit,
        bool ShouldHaveData
    );

    /// <summary>
    /// Test data object for Transactions list by transactions or default type<br/>
    /// <c>/transactions</c>
    /// </summary>
    public record TransactionsListTestDef(
        string Month,
        string Year,
        string ExpectedDate,
        int ExpectedStatusCode,
        int ExpectedTotalCount,
        int ExpectedDebit,
        int ExpectedTotalTransactions
    );

    /// <summary>
    /// Test data object for Transactions by categories<br/>
    /// <c>/transactions/category/:categoryId</c>
    /// </summary>
    public record TransactionsByCategoryIdTestDef(
        string Month,
        string Year,
        string ExpectedCategoryName,
        int ExpectedTotalDates,
        int ExpectedTotalTransactionsForDate,
        List<ByCategoryData> ExpectedTransactions
    );

    /// <summary>
    /// Test data object for Insert Transactions<br/>
    /// <c>/transactions</c>
    /// </summary>
    public record TransactionsInsertTestDef(
        int ExpectedStatusCode,
        int ExpectedHttpStatusCode,
        string ExpectedMessage,
        Transaction Transaction
    );
}

