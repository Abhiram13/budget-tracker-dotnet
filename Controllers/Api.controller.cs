using Microsoft.AspNetCore.Mvc;
using budget_tracker.Attributes;

namespace budget_tracker.Controllers
{    
    [ApiController]
    [JwtAuthourise]
    [Route("api/[controller]", Name = "[controller]_")]    
    public abstract class ApiBaseController : ControllerBase { }
}