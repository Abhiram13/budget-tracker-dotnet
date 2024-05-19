using Defination;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Services;

public static class Collection
{
    public static IMongoCollection<Transaction> Transaction {get {return Mongo.DB.GetCollection<Transaction>("transactions");}}
    public static IMongoCollection<Category> Category {get {return Mongo.DB.GetCollection<Category>("categories");}}
    public static IMongoCollection<Bank> Bank {get {return Mongo.DB.GetCollection<Bank>("banks");}}
}

public abstract class MongoService<T> : IService<T> where T : class
{
    protected IMongoCollection<T> collection;

    public MongoService(IMongoCollection<T> _collection)
    {
        collection = _collection;
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

    public async Task<T> SearchById(string Id)
    {
        FilterDefinition<T> filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(Id));

        return await collection.Find(filter).FirstAsync();
    }
}