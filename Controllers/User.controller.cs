using Microsoft.AspNetCore.Mvc;
using System.Net;
using BudgetTracker.Defination;
using BudgetTracker.Injectors;
using BudgetTracker.Security;
using BudgetTracker.Application;

namespace BudgetTracker.Controllers;

[ApiController]
public class UserController : ApiBaseController
{
    private readonly IUserService service;

    public UserController(IUserService _service)
    {
        service = _service;
    }

    [HttpPost]
    public ApiResponse<string> Add([FromBody] User body)
    {
        // AsyncCallback<string> callback = async () => {
        //     Security.HashDetails hash = Security.Hash.GenerateHashedPassword(body.Password);

        //     User user = new User()
        //     {
        //         FirstName = body.FirstName,
        //         LastName = body.LastName,
        //         UserName = body.UserName,
        //         Password = hash.Password,
        //         Salt = hash.Salt
        //     };

        //     Logger.Log(user);

        //     // await service.InserOne(user);

        //     return new ApiResponse<string>()
        //     {
        //         Message = "User added successfully",
        //         StatusCode = HttpStatusCode.Created,
        //     };
        // };

        return new ApiResponse<string>() {
            Message = "OK",
            StatusCode = HttpStatusCode.OK,
        };

        // return await Handler<string>.Exception(callback);
    }

    [HttpGet]
    public async Task Get()
    {
        List<User> users = await service.GetList();
        User user = users[0];
        bool isSame = Hash.Compare(user.Salt, "1234", user.Password);
        Logger.Log(isSame);
        return;
    }
}