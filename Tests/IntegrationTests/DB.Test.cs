using BudgetTracker.Application;
using MongoDB.Driver;
using Mongo2Go;
using Xunit;
using BudgetTracker.Defination;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IntegrationTests;

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
        _setEnvironmentVariables();
        _runner = MongoDbRunner.Start();
        _client = new MongoClient($"mongodb+srv://{Env.USERNAME}:{Env.PASSWORD}@{Env.HOST}/?retryWrites=true&w=majority&appName=Trsnactions");
        Database = _client.GetDatabase("Development");
    }

    private void _setEnvironmentVariables()
    {
        var currentParent = Directory.GetParent(Directory.GetCurrentDirectory());
        var mainParent = Directory.GetParent(currentParent.Parent.FullName);
        string root = Directory.GetCurrentDirectory();
        string dotenv = Path.Combine(mainParent.Parent.FullName, ".env");
        DotEnv.Load(dotenv);
    }

    public void Dispose()
    {
        // delete all records in "transactions" collection
        // Database?.GetCollection<Transaction>("transactions").DeleteManyAsync(FilterDefinition<Transaction>.Empty);
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
        builder.ConfigureTestServices(services => {
            ServiceDescriptor? dbDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IMongoDatabase));        
            services.Remove(dbDescriptor);

            ServiceDescriptor? clientDescriptior = services.SingleOrDefault(d => d.ServiceType == typeof(IMongoClient));
            services.Remove(clientDescriptior);
            
            services.AddSingleton(_ => _fixture.Database);
        });
    }
}

[CollectionDefinition("category")]
public class CategoryCollection : ICollectionFixture<MongoDBFixture> { }

[CollectionDefinition("transaction")]
public class TransactionCollection : ICollectionFixture<MongoDBFixture> { }

public abstract class IntegrationTests
{
    protected readonly MongoDBFixture _fixture;
    protected readonly HttpClient _client;
    protected readonly string _API_KEY = "MJAhQCrhaP5bJXUaywYUUhKgEUWb5C6HFimIpIg9zsOZCvHugRH9lgAlDi1MZ31iel5R";

    public IntegrationTests(MongoDBFixture fixture)
    {
        _fixture = fixture;
        Environment.SetEnvironmentVariable("PORT", "3002");
        Environment.SetEnvironmentVariable("ENV", "Development");
        Environment.SetEnvironmentVariable("API_KEY", _API_KEY);

        CustomWebApplicationFactory factory = new CustomWebApplicationFactory(_fixture);
        
        _client = factory.CreateClient();
    }
}