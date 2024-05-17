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
    [BsonElement("_id")]
    [JsonPropertyName("_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ObjectId Id {get; set;}
    
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
    public string? Message {get; set;}

    [JsonPropertyName("result")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Result {get; set;}
}