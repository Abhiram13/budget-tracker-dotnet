using Defination;
using MongoDB.Driver;

namespace Services;

public abstract class MongoService<T> : IService<T> where T : class
{
    protected readonly IMongoCollection<T> collection;
    protected readonly IMongoCollection<Transaction> transactionCollection;

    public MongoService(string collectionName)
    {
        collection = Mongo.DB.GetCollection<T>(collectionName);
        transactionCollection = Mongo.DB.GetCollection<Transaction>(Collection.Transaction);
    }

    public async Task InserOne(T document)
    {
        await collection.InsertOneAsync(document);
    }

    public async Task<List<T>> GetList()
    {
        FilterDefinition<T> filter = Builders<T>.Filter.Empty;
        return await collection.Aggregate().Match(filter).ToListAsync();
    }
}