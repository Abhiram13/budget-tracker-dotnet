using Defination;

namespace Services;

public class UserService : MongoServices<User>, IUserService
{
    public UserService() : base(Collection.User) { }    
}