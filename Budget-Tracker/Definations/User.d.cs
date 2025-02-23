using System.Text.Json.Serialization;

namespace BudgetTracker.Defination
{
    [Obsolete]
    public class User : MongoObject
    {
        [JsonPropertyName("user_name")]
        public string UserName { get; set; } = "";

        [JsonPropertyName("password")]
        public string Password { get; set; } = "";

        [JsonPropertyName("first_name")]
        public string FirstName { get; set; } = "";

        [JsonPropertyName("last_name")]
        public string LastName { get; set; } = "";

        [JsonPropertyName("salt")]
        public string Salt { get; set; } = "";
    }

    [Obsolete]
    public class Login
    {
        [JsonPropertyName("user_name")]
        public string Username { get; set; } = "";

        [JsonPropertyName("password")]
        public string Password { get; set; } = "";
    } 
}

namespace BudgetTracker.Interface
{
    [Obsolete]
    public interface IUserService : IMongoService<Defination.User>
    {
        Task<string?> Login(string username, string password);
    };
}