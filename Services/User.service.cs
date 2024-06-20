using Defination;
using MongoDB.Driver;
using Security;

namespace Services;

public class UserService : MongoServices<User>, IUserService
{
    public UserService() : base(Collection.User) { }

    public async Task<string?> Login(string username, string password)
    {
        Func<Task<User?>> UserWithUserName = async () => {
            FilterDefinition<User> filter = Builders<User>.Filter.Eq(u => u.UserName, username);
            User? user = await collection.Aggregate().Match(filter).FirstOrDefaultAsync();

            return user;
        };

        User? user = await UserWithUserName();

        if (user == null)
        {
            return null;
        }

        Func<bool> IsValidPassword = () => {
            string salt = user?.Salt ?? "";
            string _password = user?.Password ?? "";
            return Hash.Compare(salt, password, _password);
        };

        if (IsValidPassword())
        {
            string jwt = JWT.Service.CreateToken();
            return jwt;
        }

        return null;
    }
}