using System.Net;

namespace BudgetTracker.Models;

public record ApiResponseStatusCode
{
    [JsonPropertyName("status_code")]
    public HttpStatusCode StatusCode { get; init; }
}

public record ApiResponse : ApiResponseStatusCode
{
    [JsonPropertyName("message")]
    public required string Message { get; init; }
}

public record ApiResponse<T> : ApiResponseStatusCode where T : class
{
    [JsonPropertyName("result")]
    public required T Result { get; init; }
}