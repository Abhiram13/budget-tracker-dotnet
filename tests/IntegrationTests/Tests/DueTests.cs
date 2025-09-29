using System.Text;
using System.Text.Json;
using BudgetTracker.Core.Domain.Entities;
using BudgetTracker.Core.Domain.ValueObject;
using BudgetTracker.Core.Domain.ValueObject.Dues;
using BudgetTracker.Tests.IntegrationTests.Data.Dues;
using BudgetTracker.Tests.IntegrationTests.Definations.Dues;
using BudgetTracker.Tests.IntegrationTests.Utils;
using MongoDB.Driver;

namespace BudgetTracker.Tests.IntegrationTests;

[Collection("due")]
public class DueIntegrationTests : IntegrationTests
{
    private readonly IMongoCollection<Due> _collection;

    public DueIntegrationTests(MongoDBFixture fixture) : base(fixture)
    {
        _collection = fixture.Database.GetCollection<Due>("due");
        _client.DefaultRequestHeaders.Add("API_KEY", _API_KEY);
    }

    [Theory, ClassData(typeof(DueInsertTestData))]
    public async Task Insert_Dues_Async(DueInsertTestDef testData)
    {
        await using (DueInsertDisposals _ = new DueInsertDisposals(_fixture, _client))
        {
            string json = JsonSerializer.Serialize(testData.DueBody);
            StringContent payload = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage httpResponse = await _client.PostAsync("dues", payload);
            string response = await httpResponse.Content.ReadAsStringAsync();
            ApiResponse<string>? apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(response);

            Assert.NotNull(apiResponse);
            Assert.Equal(testData.ExpectedHttpStatusCode, (int)httpResponse.StatusCode);
            Assert.Equal(testData.ExpectedStatusCode, (int)apiResponse.StatusCode);
            Assert.Equal(testData.ExpectedMessage, apiResponse.Message);
        }
    }

    [Theory, ClassData(typeof(DueDetailsTestData))]
    public async Task Due_Details_By_Id_Async(DueDetailsTestDef data)
    {
        await using (DueInsertDisposals disposalTest = new DueInsertDisposals(_fixture, _client))
        {
            await disposalTest.InsertManyAsync();
            Due due = await _collection.Find(FilterDefinition<Due>.Empty).FirstOrDefaultAsync();
            HttpResponseMessage httpResponse = await _client.GetAsync($"/dues/{due.Id}");
            string response = await httpResponse.Content.ReadAsStringAsync();
            ApiResponse<DueDetails> apiResponse = JsonSerializer.Deserialize<ApiResponse<DueDetails>>(response)!;

            Assert.Equal(data.ExpectedHttpStatusCode, (int)httpResponse.StatusCode);
            Assert.Equal(data.ExpectedStatusCode, (int)apiResponse.StatusCode);
            Assert.NotNull(apiResponse.Result);
        }
    }
}