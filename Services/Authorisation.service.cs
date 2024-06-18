using Defination;
using Global;

namespace Services
{
    public class Authorisation
    {
        private IUserService userService;
        public Authorisation(IUserService _userService) 
        { 
            userService = _userService;
        }

        public async Task Login(string username, string password)
        {
            User? user = await userService.SearchByUserName(username);
            Logger.Log(user);
        }
    }
}