using System.Net;
using System.Text.Json.Serialization;

namespace BudgetTracker.Core.Domain.ValueObject;

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