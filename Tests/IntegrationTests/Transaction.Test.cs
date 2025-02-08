using System.Reflection;
using System.Text;
using System.Text.Json;
using BudgetTracker.API.Transactions.List;
using BudgetTracker.Defination;
using MongoDB.Driver;
using Xunit;

using TransactionByCategoryResult = BudgetTracker.API.Transactions.ByCategory.Result;
using CategoryTypeTransactionsResult = BudgetTracker.API.Transactions.List.Result;
using BudgetTracker.Application;

namespace IntegrationTests;

public class TransactionTestData
{
    public Transaction Transaction { get; set; } = new Transaction();
    public int StatusCode { get; set; }
    public int ResponseStatusCode { get; set; }
    public string ResponseMessage { get; set; } = string.Empty;
}

[Collection("transaction")]
[Trait("Category", "Transaction")]
public class TransactionIntegrationTests : IntegrationTests
{
    private readonly IMongoCollection<Transaction> _collection;
    private const string _categoryId = "665aa29b930ad7888c6766fa";
    private const string _invalidCategoryId = "66fc180b620148e2e36c0000";
    private const string _bankId = "66483fed6c7ed85fca653d05";
    private const string _invalidBankId = "66483fed6c7ed85fca650000";
    private const string _description = "Sample test transaction";
    private const string _date = "2024-09-18";    
    public static readonly TransactionTestData[] transactionTestData = new TransactionTestData[]
    {
        new () {
            Transaction = new () { Amount = 123, CategoryId = _categoryId, Date = _date, Description = _description, Due = false, FromBank = _bankId, ToBank = "", Type = TransactionType.Debit },
            ResponseMessage = "Transaction inserted successfully",
            ResponseStatusCode = 201,
            StatusCode = 200, 
        },
        new () {
            Transaction = new () { Amount = 123, CategoryId = "", Date = _date, Description = _description, Due = false, FromBank = _bankId, ToBank = "", Type = TransactionType.Debit },
            ResponseMessage = "The CategoryId field is required.",
            ResponseStatusCode = 400,
            StatusCode = 400, 
        },
        new () {
            Transaction = new () { Amount = 123, CategoryId = _categoryId, Date = _date, Description = _description, Due = false, FromBank = "", ToBank = "", Type = TransactionType.Debit },
            ResponseMessage = "Something went wrong. Please verify logs for more details",
            ResponseStatusCode = 500,
            StatusCode = 200,
        },
        new () {
            Transaction = new () { Amount = 123, CategoryId = _invalidCategoryId, Date = _date, Description = _description, Due = false, FromBank = _bankId, ToBank = "", Type = TransactionType.Debit },
            ResponseMessage = "Something went wrong. Please verify logs for more details",
            ResponseStatusCode = 500,
            StatusCode = 200,
        },
        new () {
            Transaction = new () { Amount = 123, CategoryId = _categoryId, Date = _date, Description = _description, Due = false, FromBank = _invalidBankId, ToBank = "", Type = TransactionType.Debit },
            ResponseMessage = "Something went wrong. Please verify logs for more details",
            ResponseStatusCode = 500,
            StatusCode = 200,
        },
        new () {
            Transaction = new () { Amount = 123, CategoryId = _categoryId, Date = "2024-01-011#", Description = _description, Due = false, FromBank = _bankId, ToBank = "", Type = TransactionType.Debit },
            ResponseMessage = "Please provide valid date.",
            ResponseStatusCode = 400,
            StatusCode = 400,
        },
        new () {
            Transaction = new () { Amount = 123, CategoryId = _categoryId, Date = "", Description = _description, Due = false, FromBank = _bankId, ToBank = "", Type = TransactionType.Debit },
            ResponseMessage = "The Date field is required.",
            ResponseStatusCode = 400,
            StatusCode = 400,
        },
        new () {
            Transaction = new () { Amount = 123, CategoryId = _categoryId, Date = "hasgds77y9-hdsk7-", Description = _description, Due = false, FromBank = _bankId, ToBank = "", Type = TransactionType.Debit },
            ResponseMessage = "Please provide valid date.",
            ResponseStatusCode = 400,
            StatusCode = 400,
        },
        new () {
            Transaction = new () { Amount = 123, CategoryId = _categoryId, Date = DateTime.Now.AddDays(2).ToString("yyyy-MM-dd"), Description = _description, Due = false, FromBank = _bankId, ToBank = "", Type = TransactionType.Debit },
            ResponseMessage = "Provided date is out of range or invalid.",
            ResponseStatusCode = 400,
            StatusCode = 400,
        },
        new () {
            Transaction = new () { Amount = 123, CategoryId = _categoryId, Date = _date, Description = "Sample test !", Due = false, FromBank = _bankId, ToBank = "", Type = TransactionType.Debit },
            ResponseMessage = "Please provide valid description.",
            ResponseStatusCode = 400,
            StatusCode = 400, 
        },
        new () {
            Transaction = new () { Amount = 123, CategoryId = _categoryId, Date = _date, Description = "Sample test 123", Due = false, FromBank = _bankId, ToBank = "", Type = TransactionType.Debit },
            ResponseMessage = "Transaction inserted successfully",
            ResponseStatusCode = 201,
            StatusCode = 200, 
        },
        new () {
            Transaction = new () { Amount = 123, CategoryId = _categoryId, Date = _date, Description = "ajdhsah HKHKHk %&^%", Due = false, FromBank = _bankId, ToBank = "", Type = TransactionType.Debit },
            ResponseMessage = "Please provide valid description.",
            ResponseStatusCode = 400,
            StatusCode = 400, 
        },
        new () {
            Transaction = new () { Amount = 123, CategoryId = _categoryId, Date = _date, Description = "", Due = false, FromBank = _bankId, ToBank = "", Type = TransactionType.Debit },
            ResponseMessage = "The Description field is required.",
            ResponseStatusCode = 400,
            StatusCode = 400, 
        },
    };
    public static readonly List<object[]> transactionTestDataEnumerable = transactionTestData.Select(t => new object[] {
        t.Transaction, t.ResponseMessage, t.ResponseStatusCode, t.StatusCode
    }).ToList();
    
    public TransactionIntegrationTests(MongoDBFixture fixture) : base(fixture)
    {
        _collection = fixture.Database.GetCollection<Transaction>("transactions");
        _client.DefaultRequestHeaders.Add("API_KEY", _API_KEY);
    }

    [Theory]
    [MemberData(nameof(transactionTestDataEnumerable))]
    public async Task Validate_Add_Transaction(Transaction transaction, string expectedResponseMessage, int expectedResponseCode, int expectedStatusCode)
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

    [Fact]
    public async Task Positive_Transactions_List()
    {
        HttpResponseMessage httpResponse = await _client.GetAsync("/transactions?type=transaction&month=09&year=2024");
        string jsonResponse = await httpResponse.Content.ReadAsStringAsync();
        ApiResponse<Result>? apiResponse = JsonSerializer.Deserialize<ApiResponse<Result>>(jsonResponse);
        PropertyInfo? categoryProp = apiResponse?.Result?.GetType().GetProperty("categories");
        PropertyInfo? banksProp = apiResponse?.Result?.GetType().GetProperty("banks");

        Assert.NotNull(apiResponse?.Result);
        Assert.Equal(200, (int) apiResponse.StatusCode);
        Assert.NotNull(apiResponse.Result.Transactions);
        Assert.NotNull(apiResponse?.Result?.TotalCount);
        Assert.Null(categoryProp);
        Assert.Null(banksProp);
        Assert.True(apiResponse.Result.Transactions.Count > 0);
    }

    [Fact]
    public async Task Transactions_List_With_New_Month()
    {
        HttpResponseMessage httpResponse = await _client.GetAsync("/transactions?type=transaction&month=10&year=2024");
        string jsonResponse = await httpResponse.Content.ReadAsStringAsync();
        ApiResponse<Result>? apiResponse = JsonSerializer.Deserialize<ApiResponse<Result>>(jsonResponse);
        PropertyInfo? categoryProp = apiResponse?.Result?.GetType().GetProperty("categories");
        PropertyInfo? banksProp = apiResponse?.Result?.GetType().GetProperty("banks");

        Assert.NotNull(apiResponse!.Result);
        Assert.Equal(200, (int) apiResponse.StatusCode);
        Assert.NotNull(apiResponse.Result.Transactions);
        Assert.NotNull(apiResponse?.Result.TotalCount);
        Assert.Null(categoryProp);
        Assert.Null(banksProp);
    }

    [Fact]
    public async Task Category_Wise_Transactions_For_Current_Month()
    {
        string currentMonth = DateTime.Now.Month.ToString("D2");
        string currentYear = DateTime.Now.Year.ToString();
        HttpResponseMessage httpResponse = await _client.GetAsync($"/transactions?month={currentMonth}&year={currentYear}&type=categories&sort=DESC");
        string jsonResponse = await httpResponse.Content.ReadAsStringAsync();
        ApiResponse<CategoryTypeTransactionsResult>? apiResponse = JsonSerializer.Deserialize<ApiResponse<CategoryTypeTransactionsResult>>(jsonResponse);

        Assert.Equal(200, (int) apiResponse!.StatusCode);
        Assert.NotNull(apiResponse.Result);

        if (apiResponse.Result.TotalCount > 0)
        {
            Assert.True(apiResponse.Result.Categories?.Count > 0);
            Assert.NotNull(apiResponse.Result.Categories);

            if (apiResponse.Result.Categories.Count > 0)
            {
                foreach (CategoryData category in apiResponse.Result.Categories)
                {
                    Assert.NotNull(category.Name);
                    Assert.NotEmpty(category.Name);
                    Assert.True(category.Amount > 0);
                    Assert.NotEmpty(category.CategoryId);
                    Assert.NotNull(category.CategoryId);
                }
            }
        }
    }

    [Theory]
    [InlineData("ASC")]
    [InlineData("DESC")]
    [InlineData("")]
    public async Task Descending_Sort_Check_In_Transaction_Categories(string sortOrder)
    {
        HttpResponseMessage httpResponse = await _client.GetAsync($"/transactions?type=category&month=09&year=2024&sort={sortOrder}");
        string response = await httpResponse.Content.ReadAsStringAsync();
        ApiResponse<Result>? apiResponse = JsonSerializer.Deserialize<ApiResponse<Result>>(response);
        PropertyInfo? categoryProp = apiResponse?.Result?.GetType().GetProperty("categories");
        PropertyInfo? banksProp = apiResponse?.Result?.GetType().GetProperty("banks");
        PropertyInfo? transactionProp = apiResponse?.Result?.GetType().GetProperty("transactions");
        List<CategoryData>? sortedList;

        if (sortOrder == "ASC")
        {
            sortedList = apiResponse?.Result?.Categories?.OrderBy(c => c.Amount).ToList();
        }
        else
        {
            sortedList = apiResponse?.Result?.Categories?.OrderByDescending(c => c.Amount).ToList();
        }
        
        Assert.True(apiResponse?.Result?.Categories?.Count > 0);
        Assert.Equal(sortedList, apiResponse.Result.Categories);
        Assert.Null(transactionProp);
        Assert.Null(banksProp);
        Assert.True(apiResponse.Result.TotalCount > 0);

        foreach (CategoryData category in apiResponse.Result.Categories)
        {
            Assert.NotNull(category.Name);
            Assert.NotEmpty(category.Name);            
            Assert.True(category.Amount > 0);
        }
    }

    [Fact]
    public async Task Transaction_Categories_For_Current_Month()
    {
        string currentMonth = DateTime.Now.Month.ToString("D2");
        string currentYear = DateTime.Now.Year.ToString();
        HttpResponseMessage httpResponse = await _client.GetAsync($"/transactions/category/665aa29b930ad7888c6766fa");
        string jsonResponse = await httpResponse.Content.ReadAsStringAsync();
        ApiResponse<TransactionByCategoryResult>? apiResponse = JsonSerializer.Deserialize<ApiResponse<TransactionByCategoryResult>>(jsonResponse);

        Assert.Equal(200, (int) apiResponse!.StatusCode);
        Assert.NotNull(apiResponse.Result);
        Assert.NotEmpty(apiResponse.Result.Category);
        Assert.NotNull(apiResponse.Result.CategoryData);

        foreach (BudgetTracker.API.Transactions.ByCategory.CategoryData data in apiResponse.Result.CategoryData)
        {
            DateTime date = DateTime.ParseExact(data.Date, "yyyy-MM-dd", null);
            int month = date.Month;
            int year = date.Year;

            Assert.NotNull(data.Date);
            Assert.Equal($"{month:00}", currentMonth);
            Assert.Equal($"{year}", currentYear);
            Assert.True(data.Transactions.Count() > 0);

            foreach (var transaction in data.Transactions)
            {
                Assert.True(transaction.Amount > 0);
                Assert.NotNull(transaction.Description);
            }
        }
    }

    [Fact]
    public async Task Transaction_Categories_For_Previous_Month()
    {
        HttpResponseMessage httpResponse = await _client.GetAsync($"/transactions/category/665aa29b930ad7888c6766fa?month=09&year=2024");
        string jsonResponse = await httpResponse.Content.ReadAsStringAsync();
        ApiResponse<TransactionByCategoryResult>? apiResponse = JsonSerializer.Deserialize<ApiResponse<TransactionByCategoryResult>>(jsonResponse);

        Assert.Equal(200, (int) apiResponse!.StatusCode);
        Assert.NotNull(apiResponse.Result);
        Assert.NotEmpty(apiResponse.Result.Category);
        Assert.NotNull(apiResponse.Result.CategoryData);

        foreach (BudgetTracker.API.Transactions.ByCategory.CategoryData data in apiResponse.Result.CategoryData)
        {
            DateTime date = DateTime.ParseExact(data.Date, "yyyy-MM-dd", null);
            int month = date.Month;
            int year = date.Year;

            Assert.NotNull(data.Date);
            Assert.Equal("09", $"{month:00}");
            Assert.Equal("2024", $"{year}");
            Assert.True(data.Transactions.Count() > 0);

            foreach (var transaction in data.Transactions)
            {
                Assert.True(transaction.Amount > 0);
                Assert.NotNull(transaction.Description);
            }
        }
    }
}

