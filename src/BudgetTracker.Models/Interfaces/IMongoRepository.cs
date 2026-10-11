using BudgetTracker.Models.Entities;

namespace BudgetTracker.Models.Mongo;

public interface IMongoRepository<in T> where T : MongoObject
{
    public Task InsertOneAsync(T document);
}
