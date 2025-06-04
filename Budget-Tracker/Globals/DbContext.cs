using BudgetTracker.API.Transactions.ByDate;
using BudgetTracker.Defination;
using BudgetTracker.Services;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using MongoDB.EntityFrameworkCore.Extensions;

namespace BudgetTracker.Services;

public class MongoContext : DbContext
{
    public DbSet<Category2> Categories { get; init; }
    public DbSet<Transaction2> Transactions { get; init; }

    public MongoContext(DbContextOptions options) : base(options) { }

    public static MongoContext Create(IMongoDatabase database)
    {
        return new MongoContext(new DbContextOptionsBuilder<MongoContext>().UseMongoDB(database.Client, database.DatabaseNamespace.DatabaseName).Options);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Category2>().ToCollection(Collection.CATEGORIES);
        modelBuilder.Entity<Transaction2>().ToCollection(Collection.TRANSACTIONS);
    }
}