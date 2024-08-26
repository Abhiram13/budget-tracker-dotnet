using Xunit;
using Moq;
using BudgetTracker.Defination;
using BudgetTracker.Controllers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using BudgetTracker.Injectors;

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
        ILogger<CategoryController> logger = null;
        CategoryController controller = new CategoryController(_categoryService.Object, cache, logger);
        ApiResponse<Category> result = await controller.SearcById("");
        Assert.Equal(200, (int)result.StatusCode);
    }
}