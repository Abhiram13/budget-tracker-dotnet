using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetTracker.Entities;
using MongoDB.Driver;

namespace BudgetTracker.Interfaces;

public interface IMongoDbRepository<T> where T : class
{
    Task InserOneAsync(T document);
    Task<List<T>> GetListAsync(ProjectionDefinition<T>? excludeProjection = null);
    Task<C> SearchByIdAsync<C>(string Id, IMongoCollection<C> _collection) where C : MongoObject;
    Task<T> SearchByIdAsync(string id);
    Task<bool> DeleteByIdAsync(string id);
    Task<bool> UpdateByIdAsync(string id, T document);
    Task<bool> CountByIdAsync(string id);
}