using System.Text.Json;

namespace BudgetTracker.Application;

[Obsolete(message: "This class is no longer in use",  error: true)]
public static class ResponseBytes
{
    public static byte[] Convert(object obj)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(obj);
        return bytes;
    }
}

[Obsolete(message: "This class is no longer in use",  error: true)]
public static class CustomResponse
{
    public static async Task Send(HttpContext context, object data)    
    {
        byte[] bytes = ResponseBytes.Convert(data);
        context.Response.Headers.Append("Content-Type", "application/json");
        await context.Response.Body.WriteAsync(bytes);
    }
}