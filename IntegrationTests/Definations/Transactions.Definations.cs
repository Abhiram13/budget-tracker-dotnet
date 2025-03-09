using BudgetTracker.Defination;

namespace IntegrationTests.Definations.Transactions
{
    /// <summary>
    /// Test data Object for Transactions by date <c>/transactions/date/:date</c>
    /// </summary>
    public class ByDateTestData
    {
        public string Date { get; set; }
        public int ExpectedHttpStatusCode { get; set; }
        public int ExpectedStatusCode { get; set; }
        public int ExpectedDebit { get; set; }
        public int ExpectedCredit { get; set; }
        public int ExpectedPartialDebit { get; set; }
        public int ExpectedPartialCredit { get; set; }
        public int ExpectedTotalTransactions { get; set; }
        public BudgetTracker.API.Transactions.ByDate.Transactions[] ExpectedTransactions { get; set; }
    }

    /// <summary>
    /// Test data object for Transactions by Id <c>/transactions/:id</c>
    /// </summary>
    public class TransactionByIdTestData
    {
        public string Id { get; set; }
        public int ExpectedHttpStatusCode { get; set; }
        public int ExpectedStatusCode { get; set; }
        public Transaction ExpectedResult { get; set; }
    }

    /// <summary>
    /// Test data object for Transactions by Bank <c>/transactions/bank/:bankid</c>
    /// </summary>
    public record ByBankTestData
    {
        public string BankId { get; set; }
        public string Month { get; set; }
        public string Year { get; set; }
        public int ExpectedStatusCode { get; set; }
        public int ExcpectedHttpStatusCode { get; set; }
        public int ExpectedTotalTransactions { get; set; }
        public BudgetTracker.API.Transactions.ByBank.Result ExpectedResult { get; set; }
    }
}

