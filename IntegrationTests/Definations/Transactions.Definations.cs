namespace IntegrationTests.Definations.Transactions
{
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
}

