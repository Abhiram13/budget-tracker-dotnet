using System.Reflection;
using System.Text;
using System.Text.Json;
using BudgetTracker.API.Transactions.List;
using BudgetTracker.Defination;
using MongoDB.Driver;
using Xunit;

using TransactionByCategoryResult = BudgetTracker.API.Transactions.ByCategory.Result;

namespace IntegrationTests;

[Collection("transaction")]
[Trait("Category", "Transaction")]
public class TransactionIntegrationTests : IntegrationTests
{
    private readonly IMongoCollection<Transaction> _collection;
    private const string _categoryId = "66fc180b620148e2e36c4a07";
    
    public TransactionIntegrationTests(MongoDBFixture fixture) : base(fixture)
    {
        _collection = fixture.Database.GetCollection<Transaction>("transactions");
        _client.DefaultRequestHeaders.Add("API_KEY", _API_KEY);
    }
    
    [Fact]
    public async Task Positive_Test_Add_Transaction()
    {
        Transaction transaction = new Transaction() 
        {
            Amount = 123,
            CategoryId = _categoryId,
            Date = "2024-09-18",
            Description = "Sample test transaction",
            Due = false,
            FromBank = "",
            ToBank = "",
            Type = TransactionType.Debit
        };
        string payload = JsonSerializer.Serialize(transaction);
        StringContent? payload1 = new StringContent(payload, Encoding.UTF8, "application/json");
        HttpResponseMessage data = await _client.PostAsync("transactions", payload1);
        string response = await data.Content.ReadAsStringAsync();
        ApiResponse<string>? apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(response);
        FilterDefinition<Transaction> descriptionFilter = Builders<Transaction>.Filter.Eq(t => t.Description, "Sample test transaction");
        FilterDefinition<Transaction> dateFilter = Builders<Transaction>.Filter.Eq(t => t.Date, "2024-09-18");
        List<Transaction> list = await _collection.Find(descriptionFilter & dateFilter).ToListAsync();

        Assert.Equal(201, (int) apiResponse!.StatusCode);
        Assert.NotNull(apiResponse.Message);
        Assert.True(list.Count > 0);
        Assert.Equal("2024-09-18", list?[0].Date);
        Assert.Equal("Sample test transaction", list?[0].Description);
    }

    [Theory]
    [InlineData("Sample test !", 400, false)]
    [InlineData("Sample test 123", 201, true)]
    [InlineData("ajdhsah HKHKHk %&^%", 400, false)]
    [InlineData("", 400, false)]
    public async Task Validate_Description_Test_Add_Transaction(string description, int statusCode, bool isExist)
    {
        string json = JsonSerializer.Serialize(new {
            amount = 6754,
            category_id = _categoryId,
            date = "2024-09-18",
            description = description,
            due = false,
            from_bank = "",
            to_bank = "",
            type = TransactionType.Credit
        });
        StringContent? payload1 = new StringContent(json, Encoding.UTF8, "application/json");
        HttpResponseMessage data = await _client.PostAsync("transactions", payload1);
        string response = await data.Content.ReadAsStringAsync();
        ApiResponse<string>? apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(response);
        FilterDefinition<Transaction> filter = Builders<Transaction>.Filter.Eq(t => t.Description, description);
        List<Transaction> list = await _collection.Find(filter).ToListAsync();
        
        Assert.Equal(statusCode, (int) apiResponse!.StatusCode);
        Assert.NotNull(apiResponse.Message);
        Assert.Equal(isExist, list.Count > 0);
    }

    [Theory]
    [InlineData("2024-01-011#", "Please provide valid date.", 400, false)]
    [InlineData("", "The Date field is required.", 400, false)]
    [InlineData("hasgds77y9-hdsk7-", "Please provide valid date.", 400, false)]
    [InlineData("2024-11-29", "Provided date is out of range or invalid.", 400, false)]
    [InlineData("2024-09-22", "Transaction inserted successfully", 201, true)]
    public async Task Validate_Date_Test_Add_Transaction(string date, string expectedResponse, int expectedStatusCode, bool isExist)
    {
        string json = JsonSerializer.Serialize(new {
            amount = 455,
            category_id = _categoryId,
            date = date,
            description = "asdk",
            due = false,
            from_bank = "",
            to_bank = "",
            type = TransactionType.Debit
        });
        StringContent? payload1 = new StringContent(json, Encoding.UTF8, "application/json");
        HttpResponseMessage data = await _client.PostAsync("transactions", payload1);
        string response = await data.Content.ReadAsStringAsync();
        ApiResponse<string>? apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(response);
        FilterDefinition<Transaction> filter = Builders<Transaction>.Filter.Eq(t => t.Date, date);
        List<Transaction> list = await _collection.Find(filter).ToListAsync();

        Assert.Equal(expectedStatusCode, (int) apiResponse!.StatusCode);
        Assert.NotNull(apiResponse.Message);
        Assert.Equal(expectedResponse, apiResponse.Message);
        Assert.Equal(isExist, list.Count > 0);
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
        Assert.True(apiResponse.Result.Transactions.Count == 0);
        Assert.True(apiResponse.Result.TotalCount == 0);
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
            Assert.NotEmpty(category.Name);
            Assert.NotNull(category.Name);
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

