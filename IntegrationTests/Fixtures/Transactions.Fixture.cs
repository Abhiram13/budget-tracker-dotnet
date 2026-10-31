using BudgetTracker.Entities;
using BudgetTracker.Interfaces;
using IntegrationTests.Utils;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace IntegrationTests.Fixtures;

public class TransactionFixture : DbFixture, IAsyncLifetime
{
    private readonly IMongoContext _context;
    public string CategoryId = string.Empty;
    public string BankId = string.Empty;
    
    public TransactionFixture(FinanceWebApplicationFactory factory) : base(factory)
    {
        _context = factory.Services.GetRequiredService<IMongoContext>();
    }

    public async Task InitializeAsync()
    {
        CategoryId = await CreateAndGetCategoryId();
        BankId = await CreateAndGetBankId();
    }

    public async Task DisposeAsync()
    {
        await _context.Transaction.DeleteManyAsync(Builders<Transaction>.Filter.Empty);
        await _context.Bank.DeleteManyAsync(Builders<Bank>.Filter.Empty);
        await _context.Category.DeleteManyAsync(Builders<Category>.Filter.Empty);
    }

    private async Task<string> CreateAndGetCategoryId()
    {
        Category category = Category.Create("Dairy");
        await _context.Category.InsertOneAsync(category);

        return category.Id;
    }

    private async Task<string> CreateAndGetBankId()
    {
        Bank bank = Bank.Create("SBI");
        await _context.Bank.InsertOneAsync(bank);

        return bank.Id;
    }
}