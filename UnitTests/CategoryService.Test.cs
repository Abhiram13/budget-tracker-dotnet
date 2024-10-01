using System.Text.Json;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using BudgetTracker.Defination;
using BudgetTracker.Controllers;
using BudgetTracker.Injectors;
using Xunit;
using Moq;
using BudgetTracker.Application;
using MongoDB.Driver;
using System.Net.Http.Json;
using System.Text;

namespace UnitTests;

#pragma warning disable

[Collection("category")]
public class CategoryServiceUnitTest : IntegrationTests
{
    private readonly Mock<ICategoryService> _categoryService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CategoryController>? _logger;
    private readonly CategoryController _controller;
    private readonly IMongoCollection<Category> _collection;

    public CategoryServiceUnitTest(MongoDBFixture fixture) : base (fixture)
    {
        _categoryService = new Mock<ICategoryService>();
        _cache = new MemoryCache(new MemoryCacheOptions());
        _logger = null;
        _controller = new (_categoryService.Object, _cache, _logger!);
        _collection = fixture.Database.GetCollection<Category>("categories");

        _client.DefaultRequestHeaders.Add("API_KEY", _API_KEY);
    }

    [Fact]
    public async Task SearchByIdTest()
    {
        Category expectedResult = new () {Id = "ashdls", Name = "Sample"};
        _categoryService.Setup(p => p.SearchById(expectedResult.Id)).ReturnsAsync(expectedResult);
        ApiResponse<Category> result = await _controller.SearchById(expectedResult.Id);

        PropertyInfo? resultProp = result?.GetType()?.GetProperty("Result");
        PropertyInfo? resultNameProp = resultProp?.GetType()?.GetProperty("Name");

        Assert.Multiple(() => {
            Assert.Equal(200, (int) result!.StatusCode);
            Assert.True(resultProp != null, "Result property should exist");
            Assert.True(resultNameProp != null, "Result.Name property should exist");            
            Assert.Equal(expectedResult.Id, result.Result?.Id);
            Assert.Equal(expectedResult.Name, result.Result?.Name);
        });
    }

    [Fact]
    public async Task SearchByIdErrorTest()
    {
        ApiResponse<Category> result = await _controller.SearchById("");
        PropertyInfo? resultProp = result?.GetType()?.GetProperty("Result");
        PropertyInfo? resultNameProp = resultProp?.GetType()?.GetProperty("Name");

        Assert.Multiple(() => {
            Assert.Equal(400, (int) result!.StatusCode);
            Assert.NotNull(result.Message);
            Assert.NotEmpty(result.Message);
        });
    }

    [Fact]
    public async Task Positve_Test_After_Add_Category()
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
        ApiResponse<string> apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(response);

        Assert.Equal(400, (int) apiResponse.StatusCode);
    }

    [Fact]
    public async Task No_Payload_At_Add_Category()
    {
        string payload = JsonSerializer.Serialize(new Category() {});
        StringContent? payload1 = new StringContent(payload, Encoding.UTF8, "application/json");
        HttpResponseMessage data = await _client.PostAsync("category", payload1);
        string response = await data.Content.ReadAsStringAsync();
        ApiResponse<string> apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(response);

        Assert.Equal(400, (int) apiResponse.StatusCode);
    }    

    [Fact] 
    public async Task List_Of_Categories()
    {        
        HttpResponseMessage data = await _client.GetAsync("/category");
        string result = await data.Content.ReadAsStringAsync();
        ApiResponse<List<Category>> response = JsonSerializer.Deserialize<ApiResponse<List<Category>>>(result);

        Assert.Equal(200, (int) response.StatusCode);
        Assert.NotNull(response.Result);
        Assert.True(response.Result.Count > 0);
    }
}