using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;

namespace BudgetTracker.Security
{
    namespace Jwt
    {
        public static class Service
        {
            public static Payload? Decode(string token)
            {
                try
                {
                    JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                    string[]? split = handler?.ReadToken(token)?.ToString()?.Split(".");
                    string json = split?[1] ?? "";
                    Payload? jwt = JsonSerializer.Deserialize<Payload>(json);
                    return jwt;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return null;
                }
            }

            public static string CreateToken(string id, string username)
            {
                // TODO: Generate new key and add in ENV Var
                string privateKey = "bAafd@A7d9#@F4*V!LHZs#ebKQrkE6pad2f3kj34c3dXy@";
                byte[] key = Encoding.ASCII.GetBytes(privateKey);
                SymmetricSecurityKey symmetricSecurity = new SymmetricSecurityKey(key);
                SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor {
                    Subject = new ClaimsIdentity(new Claim [] {
                        new Claim("user_id", $"{id}"),
                        new Claim("user_name", $"{username}")
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
    }
}