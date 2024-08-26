using Xunit;
using Moq;
using BudgetTracker.Defination;
using BudgetTracker.Controllers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using BudgetTracker.Injectors;
using System.Text.Json;
using System.Reflection;

namespace UnitTests;

public class UnitTest1
{
    private Mock<ICategoryService> _categoryService;

    public UnitTest1()
    {
        _categoryService = new Mock<ICategoryService>();
    }

    [Fact]
    public async Task Test1()
    {
        _categoryService.Setup(p => p.SearchById("")).ReturnsAsync(new Category() {});
        IMemoryCache cache = new MemoryCache(new MemoryCacheOptions());
        ILogger<CategoryController>? logger = null;
        CategoryController controller = new CategoryController(_categoryService.Object, cache, logger!);
        ApiResponse<Category> result = await controller.SearchById("");

        Console.WriteLine(
            JsonSerializer.Serialize(result)
        );

        PropertyInfo? resultProp = result?.GetType()?.GetProperty("Result");
        PropertyInfo? resultNameProp = resultProp?.GetType()?.GetProperty("Name");

        Assert.Multiple(() => {
            Assert.Equal(200, (int) result!.StatusCode);
            Assert.True(resultProp != null, "Result property should exist");
            Assert.True(resultNameProp != null, "Result.Name property should exist");
            Assert.NotEqual("", result?.Result?.Name);
            Assert.NotEqual("", result?.Result?.Id);
        });
    }
}