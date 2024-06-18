using System.Text.Json.Serialization;

namespace Defination
{
    public class Login
    {
        [JsonPropertyName("user_name")]
        public string Username { get; set; } = "";

        [JsonPropertyName("password")]
        public string Password { get; set; } = "";
    }
}