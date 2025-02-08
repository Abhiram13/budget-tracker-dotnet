using System.Text.Json.Serialization;

namespace BudgetTracker.Defination
{
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
    public interface IUserService : IMongoService<Defination.User>
    {
        Task<string?> Login(string username, string password);
    };
}