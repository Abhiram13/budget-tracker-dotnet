using MongoDB.Driver;
using Mongo2Go;
using Xunit;
using BudgetTracker.Defination;
using BudgetTracker.Services;

namespace UnitTests;

public class MongoDBFixture : IDisposable
{
    public MongoClient Client { get; set; }
    public MongoDbRunner Runner { get; set; }

    public MongoDBFixture()
    {
        Runner = MongoDbRunner.Start();
        Client = new MongoClient("mongodb://localhost:27017/");
    }

    public void Dispose()
    {
        Runner.Dispose();
    }
}

public class TestClass : IClassFixture<MongoDBFixture>
{
    private readonly MongoDBFixture _fixture;
    // private readonly HttpClient _client;

    public TestClass(MongoDBFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Init()
    {
        var db = _fixture.Client.GetDatabase("admin");
        var collection = db.GetCollection<Category>("categories");
        await collection.InsertOneAsync(new Category() {Name = "Sample category"});
    }
}