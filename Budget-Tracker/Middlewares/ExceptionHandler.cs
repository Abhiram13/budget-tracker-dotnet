using System.Net;
using System.Text.Json;
using BudgetTracker.Application;
using BudgetTracker.Defination;
using BudgetTracker.Interface;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTracker.Middlewares
{
    /// <summary>
    /// <see href="https://www.milanjovanovic.tech/blog/global-error-handling-in-aspnetcore-8">Global Error Handling in ASP.NET Core</see>
    /// </summary>
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
            catch (Exception exception)
            {                
                /// <summary>
                /// <see href="https://www.milanjovanovic.tech/blog/problem-details-for-aspnetcore-apis">Problem Details for ASP.NET Core APIs</see>
                /// </summary>
                ProblemDetails problemDetails = new ProblemDetails()
                {
                    Type = exception.GetType().ToString(),
                    Title = "Middleware Exception",
                    Status = (int)HttpStatusCode.InternalServerError,
                    Detail = exception.Message,
                    Instance = $"{httpContext.Request.Method} {httpContext.Request.Path.Value}"
                };
                Logger.LogError(
                    exception,
                    problemDetails                                    
                );
                ApiResponse<string> response = new ApiResponse<string>()
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = "Something went wrong. Please verify logs for more details"
                };                

                await httpContext.Response.WriteAsJsonAsync(response);
            }
        }
    }
}