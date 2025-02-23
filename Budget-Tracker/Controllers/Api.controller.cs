using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BudgetTracker.Security.Authentication;

namespace BudgetTracker.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = ApiKeySchemaOptions.DefaultSchema)]
    [Route("[controller]", Name = "[controller]_")] // TODO (work on updating controllers start point)
    public abstract class ApiBaseController : ControllerBase { }
}