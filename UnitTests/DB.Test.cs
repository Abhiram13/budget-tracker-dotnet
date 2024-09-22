using MongoDB.Driver;
using Mongo2Go;
using Xunit;
using BudgetTracker.Defination;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace UnitTests;

#pragma warning disable
/// <summary>
/// <see href="https://medium.com/tech-thesignalgroup/integration-tests-with-in-memory-mongodb-b1482ce5d179">Link</see>
/// </summary>
public class MongoDBFixture : IDisposable
{
    private MongoClient _client { get; set; }
    private MongoDbRunner _runner { get; set; }
    public IMongoDatabase Database { get; set; }

    public MongoDBFixture()
    {
        _runner = MongoDbRunner.Start();
        _client = new MongoClient("mongodb://localhost:27017/");
        Database = _client.GetDatabase("admin");
    }

    public void Dispose()
    {
        _runner.Dispose();
    }
}

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly MongoDBFixture _fixture;

    public CustomWebApplicationFactory(MongoDBFixture fixture)
    {
        _fixture = fixture;
    }

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        Console.WriteLine("this was added");        
        builder.ConfigureTestServices(services => {
            var dbDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IMongoDatabase));        
            services.Remove(dbDescriptor);

            var clientDescriptior = services.SingleOrDefault(d => d.ServiceType == typeof(IMongoClient));
            services.Remove(clientDescriptior);
            
            services.AddSingleton(_ => _fixture.Database);
        });
    }
}

public class TestClass : IClassFixture<MongoDBFixture>
{
    private readonly MongoDBFixture _fixture;
    private readonly HttpClient _client;

    public TestClass(MongoDBFixture fixture)
    {
        _fixture = fixture;
        Environment.SetEnvironmentVariable("PORT", "3002");
        Environment.SetEnvironmentVariable("ENV", "Test");
        Environment.SetEnvironmentVariable("DB", "admin");
        Environment.SetEnvironmentVariable("API_KEY", "MJAhQCrhaP5bJXUaywYUUhKgEUWb5C6HFimIpIg9zsOZCvHugRH9lgAlDi1MZ31iel5R");
        // var factory = new WebApplicationFactory<Program>()
        //     .WithWebHostBuilder(builder => {
        //         builder.ConfigureServices((services) => {
        //             services.RemoveAll<IMongoClient>();
        //             services.AddSingleton(_ => _fixture.Database);
        //         });
        //     });
        CustomWebApplicationFactory factory = new CustomWebApplicationFactory(_fixture);
        
        _client = factory.CreateClient();        
    }

    [Fact]
    public async Task Init()
    {
        // var db = _fixture.Database;
        // var collection = db.GetCollection<Category>("categories");
        // await collection.InsertOneAsync(new Category() {Name = "Sample category"});

        _client.DefaultRequestHeaders.Add("API_KEY", "MJAhQCrhaP5bJXUaywYUUhKgEUWb5C6HFimIpIg9zsOZCvHugRH9lgAlDi1MZ31iel5R");
        HttpResponseMessage data = await _client.GetAsync("/category");
        string result = await data.Content.ReadAsStringAsync();

        Console.WriteLine(data.StatusCode);
        Console.WriteLine(result);
    }
}