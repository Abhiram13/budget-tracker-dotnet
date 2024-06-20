using System.Net;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Defination;

public static class Env
{
    public static readonly string? HOST = Environment.GetEnvironmentVariable("HOST");
    public static readonly string? DB = Environment.GetEnvironmentVariable("DB");
    public static readonly string? PASSWORD = Environment.GetEnvironmentVariable("PASSWORD");
    public static readonly string? USERNAME = Environment.GetEnvironmentVariable("USERNAME");
}

public abstract class MongoObject
{   
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonElement("_id")]
    [JsonPropertyName("id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string Id {get; set;} = "";
    
    [BsonElement("__v")]
    [JsonPropertyName("__v")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public byte? V {get; set;}
}

public class ApiResponse<T> where T : class
{
    [JsonPropertyName("status_code")]
    public HttpStatusCode StatusCode {get; set;}

    [JsonPropertyName("message")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Message {get; set;}

    [JsonPropertyName("result")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Result {get; set;}
}

public interface IService<T> where T : class
{
    /// <summary>
    /// Inserts given document into the collection
    /// </summary>
    /// <param name="document">The document that needs to be inserted.</param>
    /// <typeparam name="T">The type of the document to be inserted which by default is equal to the type of the collection.</typeparam>
    /// <returns></returns>
    Task InserOne(T document);

    /// <summary>
    /// Fetches list of all documents from collection without applying any filters
    /// </summary>
    /// <typeparam name="T">Collection Type</typeparam>
    /// <returns>List of all documents from the collection without filters and joins</returns>
    Task<List<T>> GetList();

    /// <summary>
    /// Searches the document by given id in the collection
    /// </summary>
    /// <param name="id">Id of the document object_id value</param>
    /// <returns>The document if found, else null</returns>
    Task<T> SearchById(string id);
    Task<bool> DeleteById(string id);
    Task<bool> UpdateById(string id, dynamic document);
}