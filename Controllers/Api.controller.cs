using Microsoft.AspNetCore.Mvc;
using BudgetTracker.Attributes;

namespace BudgetTracker.Controllers
{
    [ApiController]
    [Authorization]
    [Route("api/[controller]", Name = "[controller]_")]
    public abstract class ApiBaseController : ControllerBase { }
}