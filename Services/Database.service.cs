using MongoDB.Driver;
using Defination;

namespace Services;

public static class Mongo
{   
    private static readonly string url = $"mongodb+srv://{Env.USERNAME}:{Env.PASSWORD}@{Env.HOST}/?retryWrites=true&w=majority&appName=Trsnactions";
    private static readonly MongoClient client = new MongoClient(url);
    public static readonly IMongoDatabase DB = client.GetDatabase(Env.DB);
}