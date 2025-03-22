using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.Json;
using BudgetTracker.Interface;
using BudgetTracker.Defination;
using BudgetTracker.Application;

namespace BudgetTracker.Services
{
    public class Collection
    {
        public const string TRANSACTIONS = "transactions";
        public const string CATEGORIES = "categories";
        public const string BANKS = "banks";
        public const string DUES = "dues";
    }

    public class MongoDBContext : IMongoContext
    {
        private readonly IMongoDatabase _database;
        
        public MongoDBContext()
        {
            string url = $"mongodb+srv://{Secrets.USERNAME}:{Secrets.PASSWORD}@{Secrets.HOST}/?retryWrites=true&w=majority&appName=Trsnactions";
            MongoClient client = new MongoClient(url);
            _database = client.GetDatabase(Secrets.DB);
        }
        
        public IMongoDatabase Database => _database;
        public IMongoCollection<Transaction> Transaction => _database.GetCollection<Transaction>(Collection.TRANSACTIONS);
        public IMongoCollection<Category> Category => _database.GetCollection<Category>(Collection.CATEGORIES);
        public IMongoCollection<Bank> Bank => _database.GetCollection<Bank>(Collection.BANKS);
        public IMongoCollection<Due> Dues => _database.GetCollection<Due>(Collection.DUES);
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

        public async Task<List<T>> GetList(ProjectionDefinition<T>? excludeProjection = null)
        {
            FilterDefinition<T> filter = Builders<T>.Filter.Empty;
            IAggregateFluent<T> aggregate = collection.Aggregate().Match(filter);

            if (excludeProjection != null)
            {
                aggregate = aggregate.Project<T>(excludeProjection);
            }

            return await aggregate.ToListAsync();
        }

        public async Task<T> SearchById(string Id)
        {
            if (string.IsNullOrEmpty(Id)) return default!;

            FilterDefinition<T> filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(Id));

            return await collection.Find(filter).FirstOrDefaultAsync();
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

        public async Task<bool> UpdateById(string id, T document)
        {
            FilterDefinition<T> filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(id));
            document.Id = id;
            ReplaceOneResult result = await collection.ReplaceOneAsync(filter, document);
            return result.ModifiedCount > 0;
        }
    }
}