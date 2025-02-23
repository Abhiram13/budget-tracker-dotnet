using System.Text.Json.Serialization;

namespace BudgetTracker.Security
{
    namespace Jwt
    {
        [Obsolete]
        public class Payload
        {
            [JsonPropertyName("user_id")]
            public string UserId { get; set; } = "";

            [JsonPropertyName("user_name")]
            public string Username { get; set; } = "";

            [JsonPropertyName("iss")]
            public string Iss { get; set; } = "";

            [JsonIgnore]
            [JsonPropertyName("nbf")]
            public string Nbf { get; set; } = "";

            [JsonIgnore]
            [JsonPropertyName("exp")]
            public string Exp { get; set; } = "";

            [JsonIgnore]
            [JsonPropertyName("iat")]
            public string Iat { get; set; } = "";
        }
    }
}