using Defination;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Net;
using System.Text.Json;

namespace Services;

public static class Collection
{
    public static IMongoCollection<Transaction> Transaction { get { return Mongo.DB.GetCollection<Transaction>("transactions"); } }
    public static IMongoCollection<Category> Category { get { return Mongo.DB.GetCollection<Category>("categories"); } }
    public static IMongoCollection<Bank> Bank { get { return Mongo.DB.GetCollection<Bank>("banks"); } }
    public static IMongoCollection<Due> Due { get { return Mongo.DB.GetCollection<Due>("dues"); } }
    public static IMongoCollection<User> User { get { return Mongo.DB.GetCollection<User>("users"); } }
}

public delegate ApiResponse<T> Callback<T>() where T : class;

/// <typeparam name="T">
///     The Type of result to the client in ApiResponse Object
/// </typeparam>
/// <returns></returns>
public delegate Task<ApiResponse<T>> AsyncCallback<T>() where T : class;

public static class Handler<T> where T : class
{
    public static ApiResponse<T> Exception(Callback<T> callback)
    {
        try
        {
            return callback();
        }
        catch (BadRequestException e)
        {
            return new ApiResponse<T>()
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = $"Bad request. Message: {e.Message}",
            };
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
        catch (BadRequestException e)
        {
            return new ApiResponse<T>()
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = $"Bad request. Message: {e.Message}",
            };
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

        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<bool> DeleteById(string id)
    {
        FilterDefinition<T> filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(id));
        DeleteResult result = await collection.DeleteOneAsync(filter);

        return result.DeletedCount > 0;
    }

    public async Task<bool> UpdateById(string id, dynamic document)
    {
        FilterDefinition<T> filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(id));        
        Dictionary<string, string> dictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(document);
        UpdateDefinition<T>? update = null;

        foreach (var prop in dictionary)
        {
            if (update == null)
            {
                update = Builders<T>.Update.Set(prop.Key, prop.Value);
            }
            else
            {
                update = update?.Set(prop.Key, prop.Value);
            }
        }

        UpdateResult result = await collection.UpdateOneAsync(filter, update);

        return result.ModifiedCount > 0;
    }
}

public class BadRequestException : Exception
{
    public BadRequestException(string message) : base (message) {}
}