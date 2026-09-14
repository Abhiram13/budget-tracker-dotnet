using IntegrationTests.Utils;
using Mongo2Go;
using MongoDB.Driver;

namespace IntegrationTests.Fixtures;

public class DbFixture
{
    private FinanceWebApplicationFactory _factory { get; }
    public HttpClient Client { get; }
    
    protected DbFixture(FinanceWebApplicationFactory factory)
    {
        _factory = factory;
        Client = factory.CreateClient();
    }
}

[CollectionDefinition(nameof(DatabaseCollection))]
public abstract class DatabaseCollection : ICollectionFixture<FinanceWebApplicationFactory>;