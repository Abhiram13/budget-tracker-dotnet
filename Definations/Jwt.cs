using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.IdentityModel.Tokens;

// bAafd@A7d9#@F4*V!LHZs#ebKQrkE6pad2f3kj34c3dXy@
namespace JWT
{
    public class Payload
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