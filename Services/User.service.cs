using Defination;
using MongoDB.Driver;

namespace Services;

public class UserService : MongoServices<User>, IUserService
{
    public UserService() : base(Collection.User) { }

    public async Task<User?> SearchByUserName(string username)
    {
        try
        {
            FilterDefinition<User> userNameFilter = Builders<User>.Filter.Eq(u => u.UserName, username);
            User user = await collection.Aggregate().Match(userNameFilter).FirstOrDefaultAsync();

            return user;
        }
        catch(Exception e)
        {
            Console.WriteLine("Exception at login service", e.Message);
            return null;
        }
    } 
}