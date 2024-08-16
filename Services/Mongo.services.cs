using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.Json;
using BudgetTracker.Injectors;
using BudgetTracker.Defination;

namespace BudgetTracker.Services
{
    public static class Mongo
    {
        private static readonly string url = $"mongodb+srv://{Env.USERNAME}:{Env.PASSWORD}@{Env.HOST}/?retryWrites=true&w=majority&appName=Trsnactions";
        private static readonly MongoClient client = new MongoClient(url);
        public static readonly IMongoDatabase DB = client.GetDatabase(Env.DB);
    }

    public static class Collection
    {
        public static IMongoCollection<Transaction> Transaction { get { return Mongo.DB.GetCollection<Transaction>("transactions"); } }
        public static IMongoCollection<Category> Category { get { return Mongo.DB.GetCollection<Category>("categories"); } }
        public static IMongoCollection<Bank> Bank { get { return Mongo.DB.GetCollection<Bank>("banks"); } }
        public static IMongoCollection<Due> Due { get { return Mongo.DB.GetCollection<Due>("dues"); } }
        public static IMongoCollection<User> User { get { return Mongo.DB.GetCollection<User>("users"); } }
    }

    public abstract class MongoServices<T> : IMongoService<T> where T : MongoObject
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

        public async Task<C> SearchById<C>(string Id, IMongoCollection<C> _collection) where C : MongoObject
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
}