using System.Text.Json;
using System;

namespace BudgetTracker.Application;

public static class ResponseBytes
{
    public static byte[] Convert(object obj)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(obj);
        return bytes;
    }
}

public static class CustomResponse
{
    public async static Task Send(HttpContext context, object data)    
    {
        byte[] bytes = ResponseBytes.Convert(data);
        context.Response.Headers.Add("Content-Type", "application/json");
        await context.Response.Body.WriteAsync(bytes);
    }
}