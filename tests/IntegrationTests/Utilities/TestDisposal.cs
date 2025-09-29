using BudgetTracker.Core.Domain.Entities;
using MongoDB.Driver;

namespace BudgetTracker.Tests.IntegrationTests.Utils;

public class TestDisposal : IAsyncDisposable
{
    protected readonly MongoDBFixture _fixture;

    public TestDisposal(MongoDBFixture fixture)
    {
        _fixture = fixture;
    }

    public virtual async ValueTask DisposeAsync()
    {
        await _fixture.Database.GetCollection<Transaction>("transactions").DeleteManyAsync(FilterDefinition<Transaction>.Empty);
    }
}