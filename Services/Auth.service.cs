using BudgetTracker.Application;
using BudgetTracker.Defination;

namespace BudgetTracker.Security
{
    namespace Authentication
    {
        public class AuthenticationMiddleware
        {
            private readonly RequestDelegate _next;

            public AuthenticationMiddleware(RequestDelegate next)
            {
                _next = next;
            }

            public async Task InvokeAsync(HttpContext context)
            {
                string? headerKey = context.Request.Headers["API_KEY"];
                string? APIKEY = Environment.GetEnvironmentVariable("API_KEY");

                if (headerKey != APIKEY)
                {
                    context.Response.StatusCode = 401;
                    ApiResponse<string> response = new ApiResponse<string>()
                    {
                        Message = "Invalid API key provided",
                        StatusCode = System.Net.HttpStatusCode.Unauthorized
                    };
                    byte[] bytes = ResponseBytes.Convert(response);
                    await context.Response.Body.WriteAsync(bytes);
                }
                else
                {
                    await _next(context);
                }
            }
        }
    }
}