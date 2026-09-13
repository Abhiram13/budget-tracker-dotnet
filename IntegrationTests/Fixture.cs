using Mongo2Go;
using MongoDB.Driver;

namespace IntegrationTests.Utils;

public class DbFixture
{
    private FinanceWebApplicationFactory _factory { get; }
    public HttpClient Client { get; }
    
    public DbFixture(FinanceWebApplicationFactory factory)
    {
        _factory = factory;
        Client = factory.CreateClient();
    }
}

[CollectionDefinition(nameof(DatabaseCollection))]
public abstract class DatabaseCollection : ICollectionFixture<FinanceWebApplicationFactory>;