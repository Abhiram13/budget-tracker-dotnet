using Microsoft.AspNetCore.Mvc;
using BudgetTracker.Attributes;

namespace BudgetTracker.Controllers
{
    [ApiController]
    // [Authorization] // TODO (work on auth from client)
    [Route("[controller]", Name = "[controller]_")] // TODO (work on updating controllers start point)
    public abstract class ApiBaseController : ControllerBase { }
}