using BudgetTracker.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTracker.Api.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = ApiKeySchemaOptions.DefaultSchema)]
[Route("[controller]", Name = "[controller]_")] // TODO (work on updating controllers start point)
public abstract class ApiBaseController : ControllerBase { }