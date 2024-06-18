using Defination;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using System.Text;
using System.Security.Claims;

public class JwtMiddleware : IMiddleware
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

public static class JwtService
{
    public static JWT.Payload? Decode(string token)
    {
        try
        {
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            string[]? split = handler?.ReadToken(token)?.ToString()?.Split(".");
            string json = split?[1] ?? "";
            JWT.Payload? jwt = JsonSerializer.Deserialize<JWT.Payload>(json);
            return jwt;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return null;
        }        
    }

    public static string CreateToken()
    {
        string privateKey = "bAafd@A7d9#@F4*V!LHZs#ebKQrkE6pad2f3kj34c3dXy@";
        byte[] key = Encoding.ASCII.GetBytes(privateKey);
        SymmetricSecurityKey symmetricSecurity = new SymmetricSecurityKey(key);
        SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor {
            Subject = new ClaimsIdentity(new[] {
                new Claim("name", "Abhiram"),
            }),
            Issuer = "Nagadi",
            Expires = DateTime.UtcNow.AddMinutes(10),
            SigningCredentials = new SigningCredentials(symmetricSecurity, SecurityAlgorithms.HmacSha256Signature)
        };
        JwtSecurityTokenHandler? tokenHandler = new JwtSecurityTokenHandler();
        SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}