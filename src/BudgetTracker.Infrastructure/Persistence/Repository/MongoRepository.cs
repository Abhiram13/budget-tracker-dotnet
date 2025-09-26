using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetTracker.Core.Application.Interfaces;
using BudgetTracker.Core.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BudgetTracker.Infrastructure.Repository;

public abstract class MongoRepository<T> : IMongoDbRepository<T> where T : MongoObject
{
    protected readonly IMongoCollection<T> _collection;

    public MongoRepository(IMongoCollection<T> collection)
    {
        _collection = collection;
    }

    public async Task<bool> CountByIdAsync(string id)
    {
        FilterDefinition<T> filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(id));
        long count = await _collection.Find(filter).CountDocumentsAsync();

        return count > 0;
    }

    public async Task<bool> DeleteByIdAsync(string id)
    {
        FilterDefinition<T> filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(id));
        DeleteResult result = await _collection.DeleteOneAsync(filter);

        return result.DeletedCount > 0;
    }

    public async Task<List<T>> GetListAsync(ProjectionDefinition<T>? excludeProjection = null)
    {
        FilterDefinition<T> filter = Builders<T>.Filter.Empty;
        IAggregateFluent<T> aggregate = _collection.Aggregate().Match(filter);

        if (excludeProjection != null)
        {
            aggregate = aggregate.Project<T>(excludeProjection);
        }

        return await aggregate.ToListAsync();
    }

    public async Task InserOneAsync(T document)
    {
        await _collection.InsertOneAsync(document);
    }

    public async Task<T> SearchByIdAsync(string id)
    {
        if (string.IsNullOrEmpty(id)) return default!;

        FilterDefinition<T> filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(id));

        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<bool> UpdateByIdAsync(string id, T document)
    {
        FilterDefinition<T> filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(id));
        document.Id = id;
        ReplaceOneResult result = await _collection.ReplaceOneAsync(filter, document);
        return result.ModifiedCount > 0;
    }

    public async Task<C> SearchByIdAsync<C>(string Id, IMongoCollection<C> _collection) where C : MongoObject
    {
        FilterDefinition<C> filter = Builders<C>.Filter.Eq("_id", ObjectId.Parse(Id));

        return await _collection.Find(filter).FirstOrDefaultAsync();
    }
}