using System.Collections;
using BudgetTracker.API.Transactions.ByCategory;
using BudgetTracker.Defination;
using IntegrationTests.Definations.Transactions;
using Result = BudgetTracker.API.Transactions.ByBank.Result;

namespace IntegrationTests.Data.Transactions
{
    public abstract class TheoryTestData<T> : TheoryData<T> where T : class
    {
        protected readonly string _categoryId = "665aa29b930ad7888c6766fa";
        protected readonly string _invalidCategoryId = "66fc180b620148e2e36c0000";
        protected readonly string _bankId = "66483fed6c7ed85fca653d05";
        protected readonly string _invalidBankId = "66483fed6c7ed85fca650000";
        protected readonly string _description = "Sample test transaction";
        protected readonly string _date = "2024-09-18";
        protected readonly string _currentMonth = DateTime.Now.ToString("MM");
        protected readonly string _currentYear = DateTime.Now.ToString("yyyy");
        protected readonly string _currentDate = DateTime.Now.ToString("yyyy-MM-dd");
        protected readonly string _categoryName = "Electricity ";
        protected readonly string _previousYear = "2024";
        protected readonly string _bankName = "Axis Bank";
    }
    
    /// <summary>
    /// <see href="https://www.milanjovanovic.tech/blog/creating-data-driven-tests-with-xunit">Creating Data-Driven Tests With xUnit</see>
    /// </summary>
    public class TransactionsListByTypeCategoryTestData : TheoryTestData<TransactionsListByCategoryTypeTestDef>
    {
        public TransactionsListByTypeCategoryTestData()
        {
            Add(new ()
            {
                Month = "09", 
                Year = _previousYear, 
                ExpectedStatusCode = 200, 
                ExpectedTotalTransactions = 2, 
                CategoryName = _categoryName, 
                ExpectedDebit = 246, 
                ShouldHaveData = true
            });
            Add(new ()
            {
                Month = _currentMonth, 
                Year = _currentYear, 
                ExpectedStatusCode = 200, 
                ExpectedTotalTransactions = 1, 
                CategoryName = _categoryName, 
                ExpectedDebit = 234, 
                ShouldHaveData = true
            });
            Add(new ()
            {
                Month = "", 
                Year = "", 
                ExpectedStatusCode = 200, 
                ExpectedTotalTransactions = 1, 
                CategoryName = _categoryName, 
                ExpectedDebit = 234, 
                ShouldHaveData = true
            });
            Add(new ()
            {
                Month = "11", 
                Year = _previousYear, 
                ExpectedStatusCode = 200, 
                ExpectedTotalTransactions = 0, 
                CategoryName = "", 
                ExpectedDebit = 0, 
                ShouldHaveData = false
            });
        }
    }

    public class TransactionsListByTypeBankTestData : TheoryTestData<TransactionsListByBankTypeTestDef>
    {
        public TransactionsListByTypeBankTestData()
        {
            Add(new (Month: "09", Year: _previousYear, ExpectedStatusCode: 200, ExpectedTotalTransactions: 2, ExpectedBankId: _bankId, ExpectedBankName: _bankName, ExpectedDebit: 246, ShouldHaveData: true));
            Add(new (Month: _currentMonth, Year: _currentYear, ExpectedStatusCode: 200, ExpectedTotalTransactions: 1, ExpectedBankId: _bankId, ExpectedBankName: _bankName, ExpectedDebit: 234, ShouldHaveData: true));
            Add(new (Month: "", Year: "", ExpectedStatusCode: 200, ExpectedTotalTransactions: 1, ExpectedBankId: _bankId, ExpectedBankName: _bankName, ExpectedDebit: 234, ShouldHaveData: true));
            Add(new (Month: "10", Year: _previousYear, ExpectedStatusCode: 200, ExpectedTotalTransactions: 0, ExpectedBankId: "", ExpectedBankName: "", ExpectedDebit: 0, ShouldHaveData: false));
        }
    }

    public class TransactionsListTestData : TheoryTestData<TransactionsListTestDef>
    {
        public TransactionsListTestData()
        {
            Add(new (Month: "09", Year: _previousYear, ExpectedStatusCode: 200, ExpectedTotalCount: 2, ExpectedDebit: 246, ExpectedDate: _date, ExpectedTotalTransactions: 2));
            Add(new (Month: "10", Year: _previousYear, ExpectedStatusCode: 200, ExpectedTotalCount: 0, ExpectedDebit: 0, ExpectedDate: "", ExpectedTotalTransactions: 0));
            Add(new (Month: _currentMonth, Year: _currentYear, ExpectedStatusCode: 200, ExpectedTotalCount: 1, ExpectedDebit: 234, ExpectedDate: _currentDate, ExpectedTotalTransactions: 1));
            Add(new (Month: "", Year: "", ExpectedStatusCode: 200, ExpectedTotalCount: 1, ExpectedDebit: 234, ExpectedDate: _currentDate, ExpectedTotalTransactions: 1));
        }
    }

    public class TransactionsByCategoryIdTestData : TheoryTestData<TransactionsByCategoryIdTestDef>
    {
        public TransactionsByCategoryIdTestData()
        {
            Add(new (Month: "09", Year: _previousYear, ExpectedCategoryName: _categoryName, ExpectedTotalDates: 1, ExpectedTotalTransactionsForDate: 2, ExpectedTransactions: new List<CategoryData>()
            {
                new () { Date = "2024-09-18", Transactions = new List<CategoryTransactions>() {
                    new () { Amount = 123, Description = _description, Type = TransactionType.Debit },
                    new () { Amount = 123, Description = _description, Type = TransactionType.Debit },
                }},
            }));
            Add(new (Month: _currentMonth, Year: _currentYear, ExpectedCategoryName: _categoryName, ExpectedTotalDates: 1, ExpectedTotalTransactionsForDate: 1, ExpectedTransactions: new List<CategoryData>()
            {
                new () { Date = _currentDate, Transactions = new List<CategoryTransactions>() {
                    new () { Amount = 234, Description = _description, Type = TransactionType.Debit },
                }},
            }));
        }
    }

    public class TransactionsByDateTestData : TheoryTestData<ByDateTestDef>
    {
        public TransactionsByDateTestData()
        {
            Add(new (
                Date: _date, 
                ExpectedStatusCode: 200, 
                ExpectedHttpStatusCode: 200, 
                ExpectedCredit: 0, 
                ExpectedDebit: 246, 
                ExpectedPartialCredit: 0, 
                ExpectedPartialDebit: 0,
                ExpectedTotalTransactions: 2,
                ExpectedTransactions: new BudgetTracker.API.Transactions.ByDate.Transactions[] {
                    new () { Amount = 123, Description = _description, Type = TransactionType.Debit, FromBank = _bankName, Category = _categoryName },
                }
            ));
            Add(new (
                Date: _currentDate, 
                ExpectedStatusCode: 200, 
                ExpectedHttpStatusCode: 200, 
                ExpectedCredit: 0, 
                ExpectedDebit: 234, 
                ExpectedPartialCredit: 0, 
                ExpectedPartialDebit: 0,
                ExpectedTotalTransactions: 1,
                ExpectedTransactions: new BudgetTracker.API.Transactions.ByDate.Transactions[] {
                    new () { Amount = 234, Description = _description, Type = TransactionType.Debit, FromBank = _bankName, Category = _categoryName },
                }
            ));
            Add(new (
                Date: "", 
                ExpectedStatusCode: 500, 
                ExpectedHttpStatusCode: 200, 
                ExpectedCredit: 0, 
                ExpectedDebit: 0, 
                ExpectedPartialCredit: 0, 
                ExpectedPartialDebit: 0,
                ExpectedTotalTransactions: 0,
                ExpectedTransactions: new BudgetTracker.API.Transactions.ByDate.Transactions[] { }
            ));
        }
    }

    public class TransactionsByBankTestData : TheoryTestData<ByBankTestDef>
    {
        public TransactionsByBankTestData()
        {
            Add(new (
                Month: "09",
                Year: _previousYear,
                BankId: _bankId,
                ExpectedStatusCode: 200,
                ExcpectedHttpStatusCode: 200,
                ExpectedTotalTransactions: 2,
                ExpectedResult: new Result()
                {
                    Bank = _bankName,
                    BankData = new List<BudgetTracker.API.Transactions.ByCategory.CategoryData>
                    {
                        new BudgetTracker.API.Transactions.ByCategory.CategoryData
                        {
                            Date = _date, 
                            Transactions = new List<CategoryTransactions>
                            {
                                new CategoryTransactions { Amount = 123, Description = _description, Type = TransactionType.Debit },
                                new CategoryTransactions { Amount = 123, Description = _description, Type = TransactionType.Debit },
                            }
                        }
                    }
                }
            ));
            Add(new (
                Month: _currentMonth,
                Year: _currentYear,
                BankId: _bankId,
                ExpectedStatusCode: 200,
                ExcpectedHttpStatusCode: 200,
                ExpectedTotalTransactions: 1,
                ExpectedResult: new Result()
                {
                    Bank = _bankName,
                    BankData = new List<BudgetTracker.API.Transactions.ByCategory.CategoryData>
                    {
                        new BudgetTracker.API.Transactions.ByCategory.CategoryData
                        {
                            Date = _currentDate, 
                            Transactions = new List<CategoryTransactions>
                            {
                                new CategoryTransactions { Amount = 234, Description = _description, Type = TransactionType.Debit },
                            }
                        }
                    }
                }
            ));
        }
    }

    public class TransactionsInsertTestData : TheoryTestData<TransactionsInsertTestDef>
    {
        public TransactionsInsertTestData()
        {
            Add(new (
                ExpectedMessage: "Transaction inserted successfully",
                ExpectedStatusCode: 201,
                ExpectedHttpStatusCode: 200,
                Transaction: new () { Amount = 123, CategoryId = _categoryId, Date = _date, Description = _description, Due = false, FromBank = _bankId, ToBank = "", Type = TransactionType.Debit }
            ));
            Add(new (
                ExpectedMessage: "The CategoryId field is required.",
                ExpectedStatusCode: 400,
                ExpectedHttpStatusCode: 400,
                Transaction: new () { Amount = 123, CategoryId = "", Date = _date, Description = _description, Due = false, FromBank = _bankId, ToBank = "", Type = TransactionType.Debit }
            ));
            Add(new (
                ExpectedMessage: "Something went wrong. Please verify logs for more details",
                ExpectedStatusCode: 500,
                ExpectedHttpStatusCode: 200,
                Transaction: new () { Amount = 123, CategoryId = _categoryId, Date = _date, Description = _description, Due = false, FromBank = "", ToBank = "", Type = TransactionType.Debit }
            ));
            Add(new (
                ExpectedMessage: "Something went wrong. Please verify logs for more details",
                ExpectedStatusCode: 500,
                ExpectedHttpStatusCode: 200,
                Transaction: new () { Amount = 123, CategoryId = _invalidCategoryId, Date = _date, Description = _description, Due = false, FromBank = _bankId, ToBank = "", Type = TransactionType.Debit }
            ));
            Add(new (
                ExpectedMessage: "Something went wrong. Please verify logs for more details",
                ExpectedStatusCode: 500,
                ExpectedHttpStatusCode: 200,
                Transaction: new () { Amount = 123, CategoryId = _categoryId, Date = _date, Description = _description, Due = false, FromBank = _invalidBankId, ToBank = "", Type = TransactionType.Debit }
            ));
            Add(new (
                ExpectedMessage: "The Date field is required.",
                ExpectedStatusCode: 400,
                ExpectedHttpStatusCode: 400,
                Transaction: new () { Amount = 123, CategoryId = _categoryId, Date = "", Description = _description, Due = false, FromBank = _bankId, ToBank = "", Type = TransactionType.Debit }
            ));
            Add(new (
                ExpectedMessage: "Please provide valid date.",
                ExpectedStatusCode: 400,
                ExpectedHttpStatusCode: 400,
                Transaction: new () { Amount = 123, CategoryId = _categoryId, Date = "2024-01-011#", Description = _description, Due = false, FromBank = _bankId, ToBank = "", Type = TransactionType.Debit }
            ));
            Add(new (
                ExpectedMessage: "Please provide valid date.",
                ExpectedStatusCode: 400,
                ExpectedHttpStatusCode: 400,
                Transaction: new () { Amount = 123, CategoryId = _categoryId, Date = "hasgds77y9-hdsk7-", Description = _description, Due = false, FromBank = _bankId, ToBank = "", Type = TransactionType.Debit }
            ));
            Add(new (
                ExpectedMessage: "Provided date is out of range or invalid.",
                ExpectedStatusCode: 400,
                ExpectedHttpStatusCode: 400,
                Transaction: new () { Amount = 123, CategoryId = _categoryId, Date = DateTime.Now.AddDays(2).ToString("yyyy-MM-dd"), Description = _description, Due = false, FromBank = _bankId, ToBank = "", Type = TransactionType.Debit }
            ));
            Add(new (
                ExpectedMessage: "Please provide valid description.",
                ExpectedStatusCode: 400,
                ExpectedHttpStatusCode: 400,
                Transaction: new () { Amount = 123, CategoryId = _categoryId, Date = _date, Description = "Sample test !", Due = false, FromBank = _bankId, ToBank = "", Type = TransactionType.Debit }
            ));
            Add(new (
                ExpectedMessage: "Transaction inserted successfully",
                ExpectedStatusCode: 201,
                ExpectedHttpStatusCode: 200,
                Transaction: new () { Amount = 123, CategoryId = _categoryId, Date = _date, Description = "Sample test 123", Due = false, FromBank = _bankId, ToBank = "", Type = TransactionType.Debit }
            ));
            Add(new (
                ExpectedMessage: "Please provide valid description.",
                ExpectedStatusCode: 400,
                ExpectedHttpStatusCode: 400,
                Transaction: new () { Amount = 123, CategoryId = _categoryId, Date = _date, Description = "ajdhsah HKHKHk %&^%", Due = false, FromBank = _bankId, ToBank = "", Type = TransactionType.Debit }
            ));
            Add(new (
                ExpectedMessage: "The Description field is required.",
                ExpectedStatusCode: 400,
                ExpectedHttpStatusCode: 400,
                Transaction: new () { Amount = 123, CategoryId = _categoryId, Date = _date, Description = "", Due = false, FromBank = _bankId, ToBank = "", Type = TransactionType.Debit }
            ));
            Add(new (
                ExpectedMessage: "Transaction inserted successfully",
                ExpectedStatusCode: 201,
                ExpectedHttpStatusCode: 200,
                Transaction: new () { Amount = 234, CategoryId = _categoryId, Date = _currentDate, Description = _description, Due = false, FromBank = _bankId, ToBank = "", Type = TransactionType.Debit }
            ));
        }
    }

    public class TransactionsByIdTestData : TheoryTestData<TransactionByIdTestDef>
    {
        public TransactionsByIdTestData()
        {
            
        }
    }
}

