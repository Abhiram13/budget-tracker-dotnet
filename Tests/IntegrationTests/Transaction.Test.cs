using System.Reflection;
using System.Text;
using System.Text.Json;
using BudgetTracker.API.Transactions.List;
using BudgetTracker.Defination;
using MongoDB.Driver;
using Xunit;

namespace IntegrationTests;

[Collection("transaction")]
[Trait("Category", "Transaction")]
public class TransactionIntegrationTests : IntegrationTests
{
    private readonly IMongoCollection<Transaction> _collection;
    
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
            Amount = 23,
            CategoryId = "665aa292930ad7888c6766f9",
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
        ApiResponse<string> apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(response);
        FilterDefinition<Transaction> descriptionFilter = Builders<Transaction>.Filter.Eq(t => t.Description, "Sample test transaction");
        FilterDefinition<Transaction> dateFilter = Builders<Transaction>.Filter.Eq(t => t.Date, "2024-09-18");
        List<Transaction> list = await _collection.Find(descriptionFilter & dateFilter).ToListAsync();

        Assert.Equal(201, (int) apiResponse.StatusCode);
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
            amount = 23,
            category_id = "665aa292930ad7888c6766f9",
            date = "2024-09-18",
            description = description,
            due = false,
            from_bank = "",
            to_bank = "",
            type = TransactionType.Debit
        });
        StringContent? payload1 = new StringContent(json, Encoding.UTF8, "application/json");
        HttpResponseMessage data = await _client.PostAsync("transactions", payload1);
        string response = await data.Content.ReadAsStringAsync();
        ApiResponse<string> apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(response);
        FilterDefinition<Transaction> filter = Builders<Transaction>.Filter.Eq(t => t.Description, description);
        List<Transaction> list = await _collection.Find(filter).ToListAsync();
        
        Assert.Equal(statusCode, (int) apiResponse.StatusCode);
        Assert.NotNull(apiResponse.Message);
        Assert.Equal(isExist, list.Count > 0);
    }

    [Theory]
    [InlineData("2024-01-011#")]
    [InlineData("ads")]
    [InlineData("hasgds77y9-hdsk7-")]
    public async Task Validate_Date_Test_Add_Transaction(string date)
    {
        string json = JsonSerializer.Serialize(new {
            amount = 23,
            category_id = "665aa292930ad7888c6766f9",
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
        ApiResponse<string> apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(response);
        FilterDefinition<Transaction> filter = Builders<Transaction>.Filter.Eq(t => t.Date, date);
        List<Transaction> list = await _collection.Find(filter).ToListAsync();

        Assert.Equal(400, (int) apiResponse.StatusCode);
        Assert.NotNull(apiResponse.Message);
        Assert.True(list.Count == 0);
    }

    [Fact]
    public async Task Positive_Transactions_List()
    {
        HttpResponseMessage httpResponse = await _client.GetAsync("/transactions?type=transaction&month=09&year=2024");
        string jsonResponse = await httpResponse.Content.ReadAsStringAsync();
        ApiResponse<Result> apiResponse = JsonSerializer.Deserialize<ApiResponse<Result>>(jsonResponse);
        PropertyInfo categoryProp = apiResponse.Result.GetType().GetProperty("categories");
        PropertyInfo banksProp = apiResponse.Result.GetType().GetProperty("banks");

        Assert.NotNull(apiResponse.Result);
        Assert.Equal(200, (int) apiResponse.StatusCode);
        Assert.NotNull(apiResponse.Result.Transactions);
        Assert.NotNull(apiResponse.Result.TotalCount);
        Assert.Null(categoryProp);
        Assert.Null(banksProp);
        Assert.True(apiResponse.Result.Transactions.Count > 0);
    }

    [Fact]
    public async Task Test_Transactions_List_With_New_Month()
    {
        HttpResponseMessage httpResponse = await _client.GetAsync("/transactions?type=transaction&month=10&year=2024");
        string jsonResponse = await httpResponse.Content.ReadAsStringAsync();
        ApiResponse<Result> apiResponse = JsonSerializer.Deserialize<ApiResponse<Result>>(jsonResponse);
        PropertyInfo categoryProp = apiResponse.Result.GetType().GetProperty("categories");
        PropertyInfo banksProp = apiResponse.Result.GetType().GetProperty("banks");

        Assert.NotNull(apiResponse.Result);
        Assert.Equal(200, (int) apiResponse.StatusCode);
        Assert.NotNull(apiResponse.Result.Transactions);
        Assert.NotNull(apiResponse.Result.TotalCount);
        Assert.Null(categoryProp);
        Assert.Null(banksProp);
        Assert.True(apiResponse.Result.Transactions.Count == 0);
    }
}

