using BudgetTracker.Models.Entities;
using BudgetTracker.Models.Mongo;
using MongoDB.Driver;

namespace BudgetTracker.Mongo.Repositories;

public abstract class MongoRepository<T> : IMongoRepository<T> where T : MongoObject
{
    protected readonly IMongoCollection<T> _collection;

    protected MongoRepository(IMongoCollection<T> collection)
    {
        _collection = collection;
    }

    public async Task InsertOneAsync(T document)
    {
        await _collection.InsertOneAsync(document).ConfigureAwait(false);
    }
}
