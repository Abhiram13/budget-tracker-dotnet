using Defination;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;

public class JwtMiddleware: IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var header = context.Request.Headers.Authorization;
        Console.WriteLine(header);
        Decode("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1lIjoiQWJoaXJhbSIsImFnZSI6IjIzNCIsIm5iZiI6MTcxODY0MDEwOCwiZXhwIjoxNzE4NjQwNzA4LCJpYXQiOjE3MTg2NDAxMDh9.MfEVbCxqH5F-ooCI8pv4tE274hcdMGaiRCCjgIJXNRU");
        await next.Invoke(context);
    }

    private void Decode(string token)
    {
        // JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
        // SecurityToken readToken = handler.ReadJwtToken(token);
        // string json = readToken.ToJson();
        // Console.WriteLine(json);
        // Jwt<Defination.JwtPayload>? jwt = JsonSerializer.Deserialize<Jwt<Defination.JwtPayload>>(json);
        // Console.WriteLine(jwt?.Payload?.Name);
    }
}