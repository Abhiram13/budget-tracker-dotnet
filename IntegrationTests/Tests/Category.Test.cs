using System.Text;
using System.Text.Json;
using BudgetTracker.Core.Domain.Entities;
using BudgetTracker.Core.Domain.ValueObject;
using MongoDB.Driver;

namespace IntegrationTests;

[Collection("category")]
public class CategoryIntegrationTests : IntegrationTests
{
    private readonly IMongoCollection<Category> _collection;
    private readonly string _categoryName;

    public CategoryIntegrationTests(MongoDBFixture fixture) : base(fixture)
    {
        _collection = fixture.Database.GetCollection<Category>("categories");
        _client.DefaultRequestHeaders.Add("API_KEY", _API_KEY);
        _categoryName = "Sample Category";
    }

    [Fact]
    public async Task Positive_Test_After_Add_Category()
    {
        string categoryName = "Test category Again";
        string payload = JsonSerializer.Serialize(new Category() { Name = categoryName });
        StringContent? payload1 = new StringContent(payload, Encoding.UTF8, "application/json");

        HttpResponseMessage data = await _client.PostAsync("category", payload1);
        FilterDefinition<Category> filter = Builders<Category>.Filter.Eq(s => s.Name, categoryName);
        List<Category> categories = await _collection.Find(filter).ToListAsync();

        Assert.True(categories.Count > 0, "Categories have data");
        Assert.Equal(categories?[0].Name, categoryName);
    }

    [Fact]
    public async Task No_Name_At_Add_Category()
    {
        string categoryName = "";
        string payload = JsonSerializer.Serialize(new Category() { Name = categoryName });
        StringContent? payload1 = new StringContent(payload, Encoding.UTF8, "application/json");
        HttpResponseMessage data = await _client.PostAsync("category", payload1);
        string response = await data.Content.ReadAsStringAsync();
        ApiResponse<string>? apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(response);

        Assert.Equal(400, (int)apiResponse!.StatusCode);
    }

    [Fact]
    public async Task No_Payload_At_Add_Category()
    {
        string payload = JsonSerializer.Serialize(new Category { });
        StringContent? payload1 = new StringContent(payload, Encoding.UTF8, "application/json");
        HttpResponseMessage data = await _client.PostAsync("category", payload1);
        string response = await data.Content.ReadAsStringAsync();
        ApiResponse<string>? apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(response);

        Assert.Equal(400, (int)apiResponse!.StatusCode);
    }

    [Fact]
    public async Task List_Of_Categories()
    {
        HttpResponseMessage data = await _client.GetAsync("/category");
        string result = await data.Content.ReadAsStringAsync();
        ApiResponse<List<Category>>? response = JsonSerializer.Deserialize<ApiResponse<List<Category>>>(result);

        Assert.Equal(200, (int)response!.StatusCode);
        Assert.NotNull(response.Result);
        Assert.True(response.Result.Count > 0);
    }

    // [Fact]
    // public async Task Delete_Category()
    // {
    //     string categoryName = "Sample Category Name";
    //     string payload = JsonSerializer.Serialize(new Category() { Name = categoryName });
    //     StringContent? payload1 = new StringContent(payload, Encoding.UTF8, "application/json");
    //     HttpResponseMessage data = await _client.PostAsync("category", payload1);
    // }
}

