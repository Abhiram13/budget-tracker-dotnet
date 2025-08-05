using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.Json;
using BudgetTracker.Interface;
using BudgetTracker.Defination;
using BudgetTracker.Application;

namespace BudgetTracker.Services
{
    /// <summary>
    /// Provides constant string values for various collection names in MongoDB
    /// </summary>
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
        protected IMongoCollection<T> _collection;

        public MongoServices(IMongoCollection<T> collection)
        {
            _collection = collection;
        }

        /// <summary>
        /// Inserts a single document into the MongoDB collection asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of the document to insert. This type should typically map to a BSON document structure in MongoDB.</typeparam>
        /// <param name="document">The document object to be inserted. This document should contain the data that will be stored in the collection.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.
        /// The task completes when the document has been successfully inserted into the database.
        /// If an error occurs during the insertion (e.g., duplicate key, network issue),
        /// the task will be faulted with the relevant exception.</returns>
        /// <exception cref="MongoWriteException">
        /// Thrown if a write concern could not be satisfied, or if there was a duplicate key error.
        /// </exception>
        /// <exception cref="MongoCommandException">
        /// Thrown if the command sent to MongoDB failed for other reasons.
        /// </exception>        
        public async Task InserOne(T document)
        {
            await _collection.InsertOneAsync(document);
        }

        public async Task<List<T>> GetList(ProjectionDefinition<T>? excludeProjection = null)
        {
            FilterDefinition<T> filter = Builders<T>.Filter.Empty;
            IAggregateFluent<T> aggregate = _collection.Aggregate().Match(filter);

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

            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<C> SearchById<C>(string Id, IMongoCollection<C> _collection) where C : MongoObject
        {
            FilterDefinition<C> filter = Builders<C>.Filter.Eq("_id", ObjectId.Parse(Id));

            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<bool> DeleteById(string id)
        {
            FilterDefinition<T> filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(id));
            DeleteResult result = await _collection.DeleteOneAsync(filter);

            return result.DeletedCount > 0;
        }

        public async Task<bool> UpdateById(string id, T document)
        {
            FilterDefinition<T> filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(id));
            document.Id = id;
            ReplaceOneResult result = await _collection.ReplaceOneAsync(filter, document);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> CountByIdAsync(string id)
        {
            FilterDefinition<T> filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(id));
            long count = await _collection.Find(filter).CountDocumentsAsync();

            return count > 0;
        }
    }
}