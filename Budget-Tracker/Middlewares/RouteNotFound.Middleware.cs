using System.Net;
using BudgetTracker.Interface;
using BudgetTracker.Defination;
using BudgetTracker.Application;

namespace BudgetTracker.Middlewares;

public class RouteNotFoundMiddleware : ICustomMiddleware
{
    private readonly RequestDelegate _next;

    public RouteNotFoundMiddleware(RequestDelegate next)
    {
        _next = next;
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

            Logger.LogWarning($"Requested endpoint [{httpContext.Request.Path.Value}] was not found.");
            httpContext.Response.ContentType = "application/json";
            await httpContext.Response.WriteAsJsonAsync(response);
        }
    }
}