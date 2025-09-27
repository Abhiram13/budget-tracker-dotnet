using System.Net;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace BudgetTracker.Defination
{
    [Obsolete("Use Abhiram.Secrets.Providers package to access secret manager", false)]
    public static class Env
    {
        public static readonly string? HOST = Environment.GetEnvironmentVariable("HOST");
        public static readonly string? DB = Environment.GetEnvironmentVariable("DB");
        public static readonly string? PASSWORD = Environment.GetEnvironmentVariable("PASSWORD");
        public static readonly string? USERNAME = Environment.GetEnvironmentVariable("USERNAME");
        public static readonly string? APP_NAME = Environment.GetEnvironmentVariable("APP_NAME");
    }

    public abstract class MongoObject
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("_id")]
        [JsonPropertyName("id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string Id { get; set; } = "";

        [BsonElement("__v")]
        [JsonPropertyName("__v")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public byte? V { get; set; }
    }

    public class ApiResponse<T> where T : class
    {
        [JsonPropertyName("status_code")]
        public HttpStatusCode StatusCode { get; set; }

        [JsonPropertyName("message")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Message { get; set; }

        [JsonPropertyName("result")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public T? Result { get; set; }
    }
}

namespace BudgetTracker.Interface
{
    /// <summary>
    /// All Common DB Actions like CRUD Actions are integrated into this service.
    /// </summary>
    /// <typeparam name="T">Collection Type</typeparam>
    public interface IMongoService<T> where T : class
    {
        Task InserOne(T document);
        Task<List<T>> GetList(ProjectionDefinition<T>? excludeProjection = null);
        Task<T> SearchById(string id);
        Task<bool> DeleteById(string id);
        Task<bool> UpdateById(string id, T document);
        Task<bool> CountByIdAsync(string id);
    }

    public interface ICustomMiddleware
    {
        Task InvokeAsync(HttpContext httpContext);
    }
}