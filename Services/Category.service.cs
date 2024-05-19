using Microsoft.AspNetCore.Mvc;
using Defination;
using Services;
using MongoDB.Driver;
using MongoDB.Bson;

namespace Services;

public class CategoryService : MongoService<Category>, ICategoryService
{
    public CategoryService() : base(Collection.Category) { }    
}