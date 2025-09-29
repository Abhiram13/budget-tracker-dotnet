using System.Text;
using System.Text.Json;
using BudgetTracker.Core.Domain.Entities;
using BudgetTracker.Core.Domain.Enums;

namespace BudgetTracker.Tests.IntegrationTests.Utils;

public class TransactionsInsertDisposals : TestDisposal
{
    private readonly HttpClient _httpClient;

    public TransactionsInsertDisposals(MongoDBFixture fixture, HttpClient httpClient) : base(fixture)
    {
        _httpClient = httpClient;
    }

    private Transaction[] GetTransactionsPayload()
    {
        string categoryId = "665aa29b930ad7888c6766fa";
        string bankId = "66483fed6c7ed85fca653d05";
        string date = "2024-09-18";
        string description = "Sample test transaction";

        return new Transaction[] {
                new () { Amount = 123, CategoryId = categoryId, Date = date, Description = description, Due = false, FromBank = bankId, ToBank = "", Type = TransactionType.Debit },
                new () { Amount = 123, CategoryId = categoryId, Date = date, Description = description, Due = false, FromBank = bankId, ToBank = "", Type = TransactionType.Debit },
                new () { Amount = 234, CategoryId = categoryId, Date = DateTime.Now.ToString("yyyy-MM-dd"), Description = description, Due = false, FromBank = bankId, ToBank = "", Type = TransactionType.Debit },
            };
    }

    public async Task InsertManyAsync()
    {
        foreach (Transaction transaction in GetTransactionsPayload())
        {
            string json = JsonSerializer.Serialize(transaction);
            StringContent? payload = new StringContent(json, Encoding.UTF8, "application/json");
            await _httpClient.PostAsync("transactions", payload);
        }
    }
}