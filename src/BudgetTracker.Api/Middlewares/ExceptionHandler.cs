using BudgetTracker.Core.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using BudgetTracker.Core.Domain.ValueObject;
using BudgetTracker.Core.Application.Exceptions;

namespace BudgetTracker.Api.Middlewares;

public class ExceptionHandlerMiddleware : ICustomMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;

    public ExceptionHandlerMiddleware(RequestDelegate requestDelegate, ILogger<ExceptionHandlerMiddleware> logger)
    {
        _next = requestDelegate;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (ValidationException ex)
        {
            _logger.LogError(ex, ex.Message);
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            ApiResponse<string> response = new ApiResponse<string>
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = ex.Message
            };
            await httpContext.Response.WriteAsJsonAsync(response);
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

            _logger.LogError(e, e.Message);
            ApiResponse<string> response = new ApiResponse<string>()
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Message = "Something went wrong. Please verify logs for more details",
            };

            await httpContext.Response.WriteAsJsonAsync(response);
        }
    }
}