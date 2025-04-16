using System.Text.Json;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using BudgetTracker.Defination;
using BudgetTracker.Controllers;
using BudgetTracker.Interface;
using Xunit;
using Moq;
using BudgetTracker.Application;
using MongoDB.Driver;
using System.Net.Http.Json;
using System.Text;

namespace UnitTests;

public class CategoryServiceUnitTest
{
    private readonly Mock<ICategoryService> _categoryService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CategoryController>? _logger;
    private readonly CategoryController _controller;

    public CategoryServiceUnitTest()
    {
        _categoryService = new Mock<ICategoryService>();
        _cache = new MemoryCache(new MemoryCacheOptions());
        _logger = null;
        _controller = new (_categoryService.Object, _cache);
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
}