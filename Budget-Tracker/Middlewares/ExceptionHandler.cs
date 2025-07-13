using System.Net;
using System.Text.Json;
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
        private readonly ILogger<ExceptionHandlerMiddleware> _logger;

        public ExceptionHandlerMiddleware(RequestDelegate requestDelegate, ILogger<ExceptionHandlerMiddleware> logger)
        {
            _logger = logger;
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
                ProblemDetails problemDetails = new ProblemDetails()
                {
                    Type = exception.GetType().ToString(),
                    Title = "Middleware Exception",
                    Status = (int) HttpStatusCode.InternalServerError,
                    Detail = exception.Message,
                };
                _logger.Log(
                    LogLevel.Error, 
                    exception, 
                    "Exception occured:- \nProblem details: {details}",                    
                    JsonSerializer.Serialize(problemDetails)
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