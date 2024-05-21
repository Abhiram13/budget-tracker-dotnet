using Defination;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Net;

namespace Services;

public static class Collection
{
    public static IMongoCollection<Transaction> Transaction {get {return Mongo.DB.GetCollection<Transaction>("transactions");}}
    public static IMongoCollection<Category> Category {get {return Mongo.DB.GetCollection<Category>("categories");}}
    public static IMongoCollection<Bank> Bank {get {return Mongo.DB.GetCollection<Bank>("banks");}}
}

public delegate ApiResponse<T> Callback<T>() where T : class;
public delegate Task<ApiResponse<T>> AsyncCallback<T>() where T : class;

public static class Handler<T> where T : class
{
    public static ApiResponse<T> Exception(Callback<T> callback)
    {
        try
        {
            return callback();
        }
        catch (Exception e)
        {
            return new ApiResponse<T>()
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Message = $"Something went wrong. Message: {e.Message}",
            };
        }
    }

    public static async Task<ApiResponse<T>> Exception(AsyncCallback<T> callback)
    {
        try
        {
            return await callback();
        }
        catch (Exception e)
        {
            return new ApiResponse<T>()
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Message = $"Something went wrong. Message: {e.Message}",
            };
        }
    }
}

public abstract class MongoServices<T> : IService<T> where T : class
{
    protected IMongoCollection<T> collection;

    public MongoServices(IMongoCollection<T> _collection)
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

    public async Task<C> SearchById<C>(string Id, IMongoCollection<C> _collection)
    {
        FilterDefinition<C> filter = Builders<C>.Filter.Eq("_id", ObjectId.Parse(Id));

        return await _collection.Find(filter).FirstAsync();
    }
}