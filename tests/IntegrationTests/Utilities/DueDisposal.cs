using System.Text;
using System.Text.Json;
using BudgetTracker.Core.Domain.Entities;
using BudgetTracker.Core.Domain.Enums;
using MongoDB.Driver;

namespace BudgetTracker.Tests.IntegrationTests.Utils;

public class DueInsertDisposals : TestDisposal
{
    private readonly HttpClient _httpClient;

    public DueInsertDisposals(MongoDBFixture fixture, HttpClient httpClient) : base(fixture)
    {
        _httpClient = httpClient;
    }

    private Due[] GetDuesPayload()
    {
        return new Due[]
        {
            new Due {
                Name = "Sample Test Due",
                Description = "This is just a sample test due description",
                Payee = "Abhi",
                PrincipalAmount = 10000,
                Status = DueStatus.Active,
                StartDate = new DateTime(2025, 9, 25),
                Comment = "Random comments :)"
            }
        };
    }

    public async Task InsertManyAsync()
    {
        foreach (Due due in GetDuesPayload())
        {
            string json = JsonSerializer.Serialize(due);
            StringContent? payload = new StringContent(json, Encoding.UTF8, "application/json");
            await _httpClient.PostAsync("dues", payload);
        }
    }

    public override async ValueTask DisposeAsync()
    {
        await _fixture.Database.GetCollection<Due>(Collection.DUES).DeleteManyAsync(FilterDefinition<Due>.Empty);
    }
}