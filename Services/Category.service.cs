using Microsoft.AspNetCore.Mvc;
using Defination;
using Services;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;

namespace Services;

public class CategoryService : MongoService<Category>, ICategoryService
{
    public CategoryService() : base(Collection.Category) { }

    public async Task<Category> SearchById(string Id)
    {
        FilterDefinition<Category> filter = Builders<Category>.Filter.Eq("_id", ObjectId.Parse(Id));

        return await collection.Find(filter).FirstAsync();
    }
}