using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.IdentityModel.Tokens;

// bAafd@A7d9#@F4*V!LHZs#ebKQrkE6pad2f3kj34c3dXy@
namespace Defination
{
    public class JwtHeader
    {
        [JsonPropertyName("alg")]
        public string Alg {get; set;} = "";

        [JsonPropertyName("typ")]
        public string Type {get; set;} = "";
    }

    public class Jwt<P>
    {
        [JsonPropertyName("_t")]
        public string T {get; set;} = "";

        [JsonPropertyName("SigningKey")]
        public dynamic? SigningKey {get; set;}

        [JsonPropertyName("_id")]
        public dynamic? Id {get; set;}

        [JsonPropertyName("InnerToken")]
        public dynamic? InnerToken {get; set;}

        [JsonPropertyName("RawAuthenticationTag")]
        public dynamic? RawAuthenticationTag {get; set;}

        [JsonPropertyName("RawCiphertext")]
        public dynamic? RawCiphertext {get; set;}

        [JsonPropertyName("RawData")]
        public string RawData {get; set;} = "";

        [JsonPropertyName("RawEncryptedKey")]
        public dynamic? RawEncryptedKey {get; set;}

        [JsonPropertyName("RawInitializationVector")]
        public dynamic? RawInitializationVector {get; set;}

        [JsonPropertyName("RawHeader")]
        public string RawHeader {get; set;} = "";

        [JsonPropertyName("RawPayload")]
        public string RawPayload {get; set;} = "";

        [JsonPropertyName("RawSignature")]
        public string RawSignature {get; set;} = "";

        [JsonPropertyName("Header")]
        public JwtHeader Header {get; set;} = new JwtHeader();

        [JsonPropertyName("Payload")]
        public P? Payload {get; set;}

        public static string Create()
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
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);        
            return tokenHandler.WriteToken(token);
        }
    }

    public class JwtPayload
    {
        [JsonPropertyName("name")]
        public string Name {get; set;} = "";

        [JsonPropertyName("iss")]
        public string Iss {get; set;} = "";

        [JsonIgnore]
        [JsonPropertyName("nbf")]
        public string Nbf {get; set;} = "";

        [JsonIgnore]
        [JsonPropertyName("exp")]
        public string Exp {get; set;} = "";

        [JsonIgnore]
        [JsonPropertyName("iat")]
        public string Iat {get; set;} = "";
    }
}