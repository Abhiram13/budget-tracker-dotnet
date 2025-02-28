using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using BudgetTracker.API.Transactions.List;
using BudgetTracker.API.Transactions.ByCategory;
using BudgetTracker.Defination;
using MongoDB.Driver;
using Xunit;
using IntegrationTests.Data.Transactions;

using TransactionByCategoryResult = BudgetTracker.API.Transactions.ByCategory.Result;
using CategoryTypeTransactionsResult = BudgetTracker.API.Transactions.List.Result;
using TransactionsByCategoryId = BudgetTracker.API.Transactions.ByCategory.CategoryData;
using ByDateTransactions = BudgetTracker.API.Transactions.ByDate;
using BudgetTracker.Application;
using BudgetTracker.Repository;
using IntegrationTests.Definations.Transactions;
using Xunit.Abstractions;

namespace IntegrationTests;

public class TransactionDisposableTests : IAsyncDisposable
{
    private readonly MongoDBFixture _fixture;
    private readonly HttpClient _client;

    public TransactionDisposableTests(MongoDBFixture fixture, HttpClient httpClient)
    {
        _fixture = fixture;
        _client = httpClient;
    }    

    private Transaction[] GetTransactionsPayload()
    {
        string categoryId = "665aa29b930ad7888c6766fa";
        string bankId = "66483fed6c7ed85fca653d05";
        string date = "2024-09-18";
        string description = "Sample test transaction";

        return new Transaction[] {
            new () { Amount = 123, CategoryId = categoryId, Date = date, Description = description, Due = false, FromBank = bankId, ToBank = "", Type = TransactionType.Debit },
            new () { Amount = 123, CategoryId = categoryId, Date = date, Description = description, Due = false, FromBank = bankId, ToBank = "", Type = TransactionType.Debit },
            new () { Amount = 234, CategoryId = categoryId, Date = DateTime.Now.ToString("yyyy-MM-dd"), Description = description, Due = false, FromBank = bankId, ToBank = "", Type = TransactionType.Debit },
        };
    }

    public async Task InsertManyAsync()
    {
        foreach (Transaction transaction in GetTransactionsPayload())
        {
            string json = JsonSerializer.Serialize(transaction);
            StringContent? payload = new StringContent(json, Encoding.UTF8, "application/json");
            await _client.PostAsync("transactions", payload);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _fixture.Database.GetCollection<Transaction>("transactions").DeleteManyAsync(FilterDefinition<Transaction>.Empty);
    }
}

[Collection("transaction")]
[Trait("Category", "Transaction")]
public class TransactionIntegrationTests : IntegrationTests
{
    private const string _categoryId = "665aa29b930ad7888c6766fa";
    private const string _invalidCategoryId = "66fc180b620148e2e36c0000";
    private const string _bankId = "66483fed6c7ed85fca653d05";
    private const string _invalidBankId = "66483fed6c7ed85fca650000";
    private const string _description = "Sample test transaction";
    private const string _date = "2024-09-18";
    public static readonly List<object[]> TransactionInsertTestData = new List<object[]>()
    {
        new object[] {
            new Transaction () { Amount = 123, CategoryId = _categoryId, Date = _date, Description = _description, Due = false, FromBank = _bankId, ToBank = "", Type = TransactionType.Debit },
            "Transaction inserted successfully", 201, 200
        },
        new object[] {
            new Transaction () { Amount = 123, CategoryId = "", Date = _date, Description = _description, Due = false, FromBank = _bankId, ToBank = "", Type = TransactionType.Debit },
            "The CategoryId field is required.", 400, 400
        },
        new object[] {
            new Transaction () { Amount = 123, CategoryId = _categoryId, Date = _date, Description = _description, Due = false, FromBank = "", ToBank = "", Type = TransactionType.Debit },
            "Something went wrong. Please verify logs for more details", 500, 200
        },
        new object[] {
            new Transaction () { Amount = 123, CategoryId = _invalidCategoryId, Date = _date, Description = _description, Due = false, FromBank = _bankId, ToBank = "", Type = TransactionType.Debit },
            "Something went wrong. Please verify logs for more details", 500, 200
        },
        new object[] {
            new Transaction () { Amount = 123, CategoryId = _categoryId, Date = _date, Description = _description, Due = false, FromBank = _invalidBankId, ToBank = "", Type = TransactionType.Debit },
            "Something went wrong. Please verify logs for more details", 500, 200
        },
        new object[] {
            new Transaction () { Amount = 123, CategoryId = _categoryId, Date = "2024-01-011#", Description = _description, Due = false, FromBank = _bankId, ToBank = "", Type = TransactionType.Debit },
            "Please provide valid date.", 400, 400
        },
        new object[] {
            new Transaction () { Amount = 123, CategoryId = _categoryId, Date = "", Description = _description, Due = false, FromBank = _bankId, ToBank = "", Type = TransactionType.Debit },
            "The Date field is required.", 400, 400
        },
        new object[] {
            new Transaction () { Amount = 123, CategoryId = _categoryId, Date = "hasgds77y9-hdsk7-", Description = _description, Due = false, FromBank = _bankId, ToBank = "", Type = TransactionType.Debit },
            "Please provide valid date.", 400, 400
        },
        new object[] {
            new Transaction () { Amount = 123, CategoryId = _categoryId, Date = DateTime.Now.AddDays(2).ToString("yyyy-MM-dd"), Description = _description, Due = false, FromBank = _bankId, ToBank = "", Type = TransactionType.Debit },
            "Provided date is out of range or invalid.", 400, 400
        },
        new object[] {
            new Transaction () { Amount = 123, CategoryId = _categoryId, Date = _date, Description = "Sample test !", Due = false, FromBank = _bankId, ToBank = "", Type = TransactionType.Debit },
            "Please provide valid description.", 400, 400
        },
        new object[] {
            new Transaction () { Amount = 123, CategoryId = _categoryId, Date = _date, Description = "Sample test 123", Due = false, FromBank = _bankId, ToBank = "", Type = TransactionType.Debit },
            "Transaction inserted successfully", 201, 200
        },
        new object[] {
            new Transaction () { Amount = 123, CategoryId = _categoryId, Date = _date, Description = "ajdhsah HKHKHk %&^%", Due = false, FromBank = _bankId, ToBank = "", Type = TransactionType.Debit },
            "Please provide valid description.", 400, 400
        },
        new object[] {
            new Transaction () { Amount = 123, CategoryId = _categoryId, Date = _date, Description = "", Due = false, FromBank = _bankId, ToBank = "", Type = TransactionType.Debit },
            "The Description field is required.", 400, 400
        },
        new object[] {
            new Transaction () { Amount = 234, CategoryId = _categoryId, Date = DateTime.Now.ToString("yyyy-MM-dd"), Description = _description, Due = false, FromBank = _bankId, ToBank = "", Type = TransactionType.Debit },
            "Transaction inserted successfully", 201, 200
        },
    };
    public static readonly List<object[]> ListTestData = new List<object[]>()
    {
        // month, year, statuscode, totalcount, debit, date, count
        new object[] { "09", "2024", 200, 2, 246, "2024-09-18", 2 },
        new object[] { "10", "2024", 200, 0, 0, "", 0 },
        new object[] { DateTime.Now.ToString("MM"), DateTime.Now.ToString("yyyy"), 200, 1, 234, DateTime.Now.ToString("yyyy-MM-dd"), 1 },
        new object[] { "", "", 200, 1, 234, DateTime.Now.ToString("yyyy-MM-dd"), 1 },
    };
    public static readonly List<object[]> TransactionListTypeCategoryTestData = new List<object[]>()
    {
        // month, year, statusCode, totalCount, category, amount, shouldHaveData
        new object[] {"09", "2024", 200, 2, "Electricity ", 246, true},        
        new object[] {DateTime.Now.ToString("MM"), DateTime.Now.ToString("yyyy"), 200, 1, "Electricity ", 234, true},
        new object[] {"", "", 200, 1, "Electricity ", 234, true},
        new object[] {"11", "2024", 200, 0, "", 0, false},
    };
    public static readonly List<object[]> TransactionsListTypeBanksTestData = new List<object[]>()
    {
        // month, year, expectedStatusCode, expectedTotalCount, expectedBankId, expectedBankName, expectedTotalDebit, expectedIsDataExist
        new object[] { "09", "2024", 200, 2, "66483fed6c7ed85fca653d05", "Axis Bank", 246, true },
        new object[] { DateTime.Now.ToString("MM"), DateTime.Now.ToString("yyyy"), 200, 1, "66483fed6c7ed85fca653d05", "Axis Bank", 234, true },
        new object[] { "", "", 200, 1, "66483fed6c7ed85fca653d05", "Axis Bank", 234, true },
        new object[] { "10", "2024", 200, 0, "", "", 0, false },
    };
    public static readonly List<object[]> TransactionListByCategoryIdTestData = new List<object[]>()
    {
        // month, year, category name, total dates for that month, total transactions for that date, data
        new object[] { "09", "2024", "Electricity ", 1, 2,
            new List<TransactionsByCategoryId>() {
                new () { Date = "2024-09-18", Transactions = new List<CategoryTransactions>() {
                    new () { Amount = 123, Description = _description, Type = TransactionType.Debit },
                    new () { Amount = 123, Description = _description, Type = TransactionType.Debit },
                }},
            }
        },
        new object[] { DateTime.Now.ToString("MM"), DateTime.Now.ToString("yyyy"), "Electricity ", 1, 1,
            new List<TransactionsByCategoryId>() {
                new () { Date = DateTime.Now.ToString("yyyy-MM-dd"), Transactions = new List<CategoryTransactions>() {
                    new () { Amount = 234, Description = _description, Type = TransactionType.Debit },
                }},
            }
        }
    };
    public static readonly List<object[]> TransactionsByDateTestData = new List<object[]>()
    {
        new object[] 
        {
            new ByDateTestData()
            {
                Date = _date,
                ExpectedStatusCode = 200, 
                ExpectedHttpStatusCode = 200,
                ExpectedCredit = 0, 
                ExpectedDebit = 246, 
                ExpectedPartialCredit = 0,
                ExpectedPartialDebit = 0,
                ExpectedTotalTransactions = 2,
                ExpectedTransactions = new []
                {
                    new ByDateTransactions.Transactions { Amount = 123, Description = _description, Type = TransactionType.Debit, FromBank = "Axis Bank", Category = "Electricity " }
                }
            }
        },
        new object[] 
        {
            new ByDateTestData()
            {
                Date = DateTime.Now.ToString("yyyy-MM-dd"),
                ExpectedStatusCode = 200, 
                ExpectedHttpStatusCode = 200,
                ExpectedCredit = 0, 
                ExpectedDebit = 234, 
                ExpectedPartialCredit = 0,
                ExpectedPartialDebit = 0,
                ExpectedTotalTransactions = 1,
                ExpectedTransactions = new []
                {
                    new ByDateTransactions.Transactions { Amount = 234, Description = _description, Type = TransactionType.Debit, FromBank = "Axis Bank", Category = "Electricity " }
                }
            }
        },
        new object[] 
        {
            new ByDateTestData()
            {
                Date = "",
                ExpectedStatusCode = 500, 
                ExpectedHttpStatusCode = 200,
                ExpectedCredit = 0, 
                ExpectedDebit = 0, 
                ExpectedPartialCredit = 0,
                ExpectedPartialDebit = 0,
                ExpectedTotalTransactions = 0,
                ExpectedTransactions = new ByDateTransactions.Transactions[] {}
            }
        }
    };
    
    public TransactionIntegrationTests(MongoDBFixture fixture) : base(fixture)
    {
        _client.DefaultRequestHeaders.Add("API_KEY", _API_KEY);
    }

    [Theory]
    [MemberData(nameof(TransactionInsertTestData))]
    public async Task Insert_Transactions(Transaction transaction, string expectedResponseMessage, int expectedResponseCode, int expectedStatusCode)
    {
        string payload = JsonSerializer.Serialize(transaction);
        StringContent? payload1 = new StringContent(payload, Encoding.UTF8, "application/json");
        HttpResponseMessage data = await _client.PostAsync("transactions", payload1);
        string response = await data.Content.ReadAsStringAsync();
        ApiResponse<string>? apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(response);

        Assert.Equal(expectedResponseCode, (int) apiResponse!.StatusCode);
        Assert.NotNull(apiResponse.Message);
        Assert.Equal(expectedResponseMessage, apiResponse.Message);
        Assert.Equal(expectedStatusCode, (int) data.StatusCode);
    }

    #region Transactions List APIs
    [Theory]
    [MemberData(nameof(TransactionListTypeCategoryTestData))]
    public async Task Transactions_List_Type_Categories(string month, string year, int expectedStatusCode, int expectedTotalCount, string expectedCategoryName, int expectedDebit, bool expectedDataExist)
    {
        await using (TransactionDisposableTests disposableTests = new TransactionDisposableTests(_fixture, _client))
        {
            await disposableTests.InsertManyAsync();
            HttpResponseMessage httpResponse = await _client.GetAsync($"/transactions?month={month}&year={year}&type=category");
            string jsonResponse = await httpResponse.Content.ReadAsStringAsync();
            ApiResponse<CategoryTypeTransactionsResult>? apiResponse = JsonSerializer.Deserialize<ApiResponse<CategoryTypeTransactionsResult>>(jsonResponse);
            PropertyInfo? transactionsProp = apiResponse?.Result?.GetType().GetProperty("transactions");
            PropertyInfo? banksProp = apiResponse?.Result?.GetType().GetProperty("banks");
            Assert.Equal(expectedStatusCode, (int) apiResponse!.StatusCode);
            Assert.NotNull(apiResponse.Result);
            Assert.Equal(expectedTotalCount, apiResponse.Result.TotalCount);
            Assert.Equal(expectedDataExist, apiResponse.Result.Categories?.Count > 0);
            Assert.Null(transactionsProp);
            Assert.Null(banksProp);

            if (apiResponse.Result.TotalCount > 0)
            {
                Assert.NotNull(apiResponse.Result.Categories);
                if (apiResponse.Result.Categories.Count > 0)
                {
                    foreach (BudgetTracker.API.Transactions.List.CategoryData category in apiResponse.Result.Categories)
                    {
                        Assert.NotNull(category.Name);
                        Assert.NotEmpty(category.Name);
                        Assert.True(category.Amount > 0);
                        Assert.NotEmpty(category.CategoryId);
                        Assert.NotNull(category.CategoryId);
                        Assert.Equal(expectedCategoryName, category.Name);
                        Assert.Equal(expectedDebit, category.Amount);
                    }
                }
            }
        }
    }
    
    [Theory]
    [MemberData(nameof(ListTestData))]
    public async Task Transactions_List_Type_Transactions(string month, string year, int expectedStatusCode, int expectedTotalCount, int expectedDebit, string expectedDate, int expectedCount)
    {
        await using (TransactionDisposableTests disposableTest = new TransactionDisposableTests(_fixture, _client))
        {
            await disposableTest.InsertManyAsync();
            HttpResponseMessage httpResponse = await _client.GetAsync($"/transactions?month={month}&year={year}");
            string jsonResponse = await httpResponse.Content.ReadAsStringAsync();
            ApiResponse<CategoryTypeTransactionsResult>? apiResponse = JsonSerializer.Deserialize<ApiResponse<CategoryTypeTransactionsResult>>(jsonResponse);
            PropertyInfo? categoryProp = apiResponse?.Result?.GetType().GetProperty("categories");
            PropertyInfo? banksProp = apiResponse?.Result?.GetType().GetProperty("banks");

            Assert.NotNull(apiResponse?.Result);
            Assert.Equal(expectedStatusCode, (int)apiResponse.StatusCode);
            Assert.NotNull(apiResponse.Result.Transactions);
            Assert.NotNull(apiResponse?.Result?.TotalCount);
            Assert.Null(categoryProp);
            Assert.Null(banksProp);
            Assert.Equal(expectedTotalCount, apiResponse.Result.TotalCount);

            if (apiResponse.Result.Transactions.Count > 0)
            {
                List<TransactionDetails> details = apiResponse.Result.Transactions;
                Assert.Contains(details, t => t.Date == expectedDate);
                int index = details.FindIndex(t => t.Date == expectedDate);

                Assert.Equal(expectedDebit, details[index].Debit);
                Assert.Equal(expectedDate, details[index].Date);
                Assert.Equal(expectedCount, details[index].Count);
            }
        }
    }

    [Theory]
    [MemberData((nameof(TransactionsListTypeBanksTestData)))]
    public async Task Transactions_List_Type_Banks(string month, string year, int expectedStatusCode, int expectedTotalCount, string expectedBankId, string expectedBankName, int expectedDebit, bool expectedIsDataExist)
    {
        await using (TransactionDisposableTests disposableTests = new TransactionDisposableTests(_fixture, _client))
        {
            await disposableTests.InsertManyAsync();
            HttpResponseMessage httpResponse = await _client.GetAsync($"/transactions?month={month}&year={year}&type=bank");
            string jsonResponse = await httpResponse.Content.ReadAsStringAsync();
            ApiResponse<BudgetTracker.API.Transactions.List.Result> apiResponse = JsonSerializer.Deserialize<ApiResponse<BudgetTracker.API.Transactions.List.Result>>(jsonResponse);
            PropertyInfo? categoryProp = apiResponse?.Result?.GetType().GetProperty("categories");
            PropertyInfo? transactionProp = apiResponse?.Result?.GetType().GetProperty("transactions");
            
            Assert.NotNull(apiResponse?.Result);
            Assert.Equal(expectedStatusCode, (int)apiResponse.StatusCode);
            Assert.NotNull(apiResponse.Result.TotalCount);
            Assert.NotNull(apiResponse.Result.Banks);
            Assert.Null(categoryProp);
            Assert.Null(transactionProp);
            Assert.Equal(expectedTotalCount, apiResponse.Result.TotalCount);
            Assert.Equal(expectedIsDataExist, apiResponse.Result.Banks.Count > 0);

            if (apiResponse.Result.Banks.Count > 0)
            {
                foreach (var bank in apiResponse.Result.Banks)
                {
                    Assert.NotNull(bank.Name);
                    Assert.Equal(expectedBankName, bank.Name);
                    Assert.Equal(expectedDebit, bank.Amount);
                    Assert.Equal(expectedBankId, bank.BankId);
                }
            }
        }
    }
    #endregion

    [Theory]
    [MemberData(nameof(TransactionListByCategoryIdTestData))]
    public async Task Transactions_By_Category(string month, string year, string expectedCategoryName, int expectedMonthRecords, int expectedTransactions, List<TransactionsByCategoryId> data)
    {
        await using (TransactionDisposableTests disposableTests = new TransactionDisposableTests(_fixture, _client))
        {
            await disposableTests.InsertManyAsync();
            HttpResponseMessage httpResponse = await _client.GetAsync($"/transactions/category/{_categoryId}?month={month}&year={year}");
            string jsonResponse = await httpResponse.Content.ReadAsStringAsync();
            ApiResponse<TransactionByCategoryResult>? apiResponse = JsonSerializer.Deserialize<ApiResponse<TransactionByCategoryResult>>(jsonResponse);

            Assert.Equal(200, (int) apiResponse!.StatusCode);
            Assert.NotEmpty(apiResponse!.Result!.Category);
            Assert.Equal(expectedCategoryName, apiResponse!.Result!.Category);
            Assert.Equal(expectedMonthRecords, apiResponse!.Result!.CategoryData.Count);

            if (apiResponse.Result.CategoryData.Count > 0)
            {
                foreach (TransactionsByCategoryId? categoryData in apiResponse.Result.CategoryData)
                {
                    Assert.Equal(expectedTransactions, categoryData.Transactions.Count);
                    Assert.Contains(data, d => d.Date == categoryData.Date);                                        
                }
            }
        }
    }

    [Theory]
    [MemberData(nameof(TransactionsByDateTestData))]
    public async Task Transactions_By_Date(ByDateTestData data)
    {
        await using (TransactionDisposableTests disposableTests = new TransactionDisposableTests(_fixture, _client))
        {
            await disposableTests.InsertManyAsync();
            string dateInput = string.IsNullOrEmpty(data.Date) ? data.Date : "";
            HttpResponseMessage httpResponse = await _client.GetAsync($"/transactions/date/{dateInput}");
            string jsonResponse = await httpResponse.Content.ReadAsStringAsync();
            ApiResponse<ByDateTransactions.Data>? apiResponse = JsonSerializer.Deserialize<ApiResponse<ByDateTransactions.Data>>(jsonResponse);
            PropertyInfo? debitProp = apiResponse?.Result?.GetType().GetProperty("Debit");
            PropertyInfo? creditProp = apiResponse?.Result?.GetType().GetProperty("Credit");
            PropertyInfo? partialDebitProp = apiResponse?.Result?.GetType().GetProperty("PartialDebit");
            PropertyInfo? partialCreditProp = apiResponse?.Result?.GetType().GetProperty("PartialCredit");
            PropertyInfo? transactionsProp = apiResponse?.Result?.GetType().GetProperty("Transactions");
            PropertyInfo? messageProp = apiResponse?.GetType().GetProperty("Message");

            if (apiResponse.StatusCode != HttpStatusCode.OK)
            {
                Assert.NotNull(messageProp);
                Assert.NotNull(apiResponse.Message);
                return;
            }
            
            Assert.Equal(data.ExpectedHttpStatusCode, (int) httpResponse.StatusCode);
            Assert.Equal(data.ExpectedStatusCode, (int) apiResponse.StatusCode);
            Assert.Equal(data.ExpectedCredit, apiResponse.Result.Credit);
            Assert.Equal(data.ExpectedDebit, apiResponse.Result.Debit);
            Assert.Equal(data.ExpectedPartialCredit, apiResponse.Result.PartialCredit);
            Assert.Equal(data.ExpectedPartialDebit, apiResponse.Result.PartialDebit);
            Assert.Equal(data.ExpectedTotalTransactions, apiResponse.Result.Transactions.Count);
            Assert.NotNull(debitProp);
            Assert.NotNull(creditProp);
            Assert.NotNull(partialDebitProp);
            Assert.NotNull(partialCreditProp);
            Assert.NotNull(transactionsProp);

            if (apiResponse.Result.Transactions.Count > 0)
            {
                foreach (ByDateTransactions.Transactions transaction in apiResponse.Result.Transactions)
                {
                    Assert.True(data.ExpectedTransactions.Any(expTransaction => expTransaction.Description == transaction.Description));
                    Assert.True(data.ExpectedTransactions.Any(expTransaction => expTransaction.Category == transaction.Category));
                    Assert.True(data.ExpectedTransactions.Any(expTransaction => expTransaction.Amount == transaction.Amount));
                    Assert.True(data.ExpectedTransactions.Any(expTransaction => expTransaction.FromBank == transaction.FromBank));
                    Assert.True(data.ExpectedTransactions.Any(expTransaction => expTransaction.Type == transaction.Type));
                    Assert.NotNull(transaction.TransactionId);
                    
                }
            }
        }
    }
}