using BudgetTracker.Middlewares;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BudgetTracker.Extensions;

public static class WebApplicationExtension
{
    extension(WebApplication application)
    {
        public WebApplication UseApplicationServices()
        {
            application
                .UseMiddlewares()
                .ConfigureAppServices()
                .MapHealthChecks();

            return application;
        }
        
        private WebApplication UseMiddlewares()
        {
            application.UseMiddleware<ExceptionHandlerMiddleware>();
            application.UseMiddleware<TraceIdMiddleware>();
        
            return application;
        }

        private WebApplication ConfigureAppServices()
        {
            application.UseAuthentication();
            application.UseAuthorization();
            application.UseHttpsRedirection();
            application.UseCors();
            application.MapControllers();
        
            return application;
        }

        private WebApplication MapHealthChecks()
        {
            application.MapHealthChecks("/health", new HealthCheckOptions () {
                ResultStatusCodes = {
                    [HealthStatus.Healthy] = StatusCodes.Status200OK,
                    [HealthStatus.Unhealthy] = StatusCodes.Status200OK,
                    [HealthStatus.Degraded] = StatusCodes.Status500InternalServerError,
                },
            });

            return application;
        }
    }
}

