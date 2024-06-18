using Microsoft.AspNetCore.Mvc;
using Defination;
using System.Net;
using Services;
using Global;

namespace budget_tracker.Controllers;

[ApiController]
[Route("")]
public class AuthorisationController : ControllerBase
{
    private readonly IUserService service;

    public AuthorisationController(IUserService _service)
    {
        service = _service;
    }

    [HttpPost]
    public async Task Add([FromBody] Login body)
    {
        await new Authorisation(service).Login(body.Username, body.Password);
        return;
    }
}