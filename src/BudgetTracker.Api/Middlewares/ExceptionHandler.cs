using BudgetTracker.Core.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using BudgetTracker.Core.Domain.ValueObject;

namespace BudgetTracker.Api.Middlewares;

public class ExceptionHandlerMiddleware : ICustomMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlerMiddleware(RequestDelegate requestDelegate)
    {
        _next = requestDelegate;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception e)
        {
            /// <summary>
            /// <see href="https://www.milanjovanovic.tech/blog/problem-details-for-aspnetcore-apis">Problem Details for ASP.NET Core APIs</see>
            /// </summary>
            ProblemDetails problemDetails = new ProblemDetails()
            {
                Type = e.GetType().ToString(),
                Title = "Middleware Exception",
                Status = (int)HttpStatusCode.InternalServerError,
                Detail = e.Message,
                Instance = $"{httpContext.Request.Method} {httpContext.Request.Path.Value}"
            };

            // FIXME: Fix the response which is failing the test cases
            ApiResponse<ProblemDetails> response = new ApiResponse<ProblemDetails>()
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Message = "Something went wrong. Please verify logs for more details",
                // Result = problemDetails
            };

            await httpContext.Response.WriteAsJsonAsync(response);
        }
    }
}