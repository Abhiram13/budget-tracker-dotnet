using System.Net;
using BudgetTracker.Interface;
using BudgetTracker.Defination;

namespace BudgetTracker.Middlewares;

public class RouteNotFoundMiddleware : ICustomMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RouteNotFoundMiddleware> _logger;

    public RouteNotFoundMiddleware(RequestDelegate next, ILogger<RouteNotFoundMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        await _next(httpContext);

        if (httpContext.Response.StatusCode == (int)HttpStatusCode.NotFound && !httpContext.Response.HasStarted)
        {
            ApiResponse<string> response = new ApiResponse<string>()
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "The requested resource was not found."
            };

            _logger.LogWarning($"Requested endpoint [{httpContext.Request.Path}] was not found.");
            httpContext.Response.ContentType = "application/json";
            await httpContext.Response.WriteAsJsonAsync(response);
        }
    }
}