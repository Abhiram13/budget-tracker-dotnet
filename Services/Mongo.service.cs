using Defination;
using MongoDB.Driver;

namespace Services;

public abstract class MongoService<T> : IService<T> where T : class
{
    private readonly IMongoCollection<T> collection;

    public MongoService(string collectionName)
    {
        collection = Mongo.DB.GetCollection<T>(collectionName);
    }

    public async Task InserOne(T document)
    {
        await collection.InsertOneAsync(document);
    }
}