using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using BudgetTracker.Services;
using BudgetTracker.Interface;
using BudgetTracker.Defination;
using BudgetTracker.Application;
using BudgetTracker.Middlewares;
using BudgetTracker.Security.Authentication;
using Google.Cloud.Diagnostics.AspNetCore3;
using Google.Cloud.Diagnostics.Common;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
string root = Directory.GetCurrentDirectory();
string dotenv = Path.Combine(root, ".env");
DotEnv.Load(dotenv);
ILoggerFactory factory;
ILogger? logger;

// Add services to the container.
builder.Configuration.AddEnvironmentVariables().Build();
builder.Logging.ClearProviders();

if (Environment.GetEnvironmentVariable("ENV") == "Development" || Environment.GetEnvironmentVariable("ENV") == "Test")
{
    builder.Logging.AddConsole();
    factory = LoggerFactory.Create(log =>
    {
        // log.AddConsole();
        log.AddSimpleConsole(options =>
        {
            options.IncludeScopes = true;
            options.SingleLine = true;
            options.TimestampFormat = "HH:mm:ss ";
            options.IncludeScopes = true;
        });
    });
    logger = factory.CreateLogger("Program");
}
else
{
    builder.Logging.AddGoogle();
    factory = LoggerFactory.Create(log => log.AddGoogle());
    logger = factory.CreateLogger("Program");
    builder.Services.AddGoogleDiagnosticsForAspNetCore();
}

builder.Services.AddSingleton<IMongoContext, MongoDBContext>();
builder.Services.AddControllers().ConfigureApiBehaviorOptions(options => {
    options.SuppressModelStateInvalidFilter = false;
    options.InvalidModelStateResponseFactory = action => {
        KeyValuePair<string, ModelStateEntry?> modelState = action.ModelState.FirstOrDefault();
        string errorAt = modelState.Key;
        string errorMessage = modelState.Value?.Errors?[0].ErrorMessage ?? $"Something went wrong at {errorAt}";            
        return new BadRequestObjectResult(new ApiResponse<string> {Message = errorMessage, StatusCode = HttpStatusCode.BadRequest});
    };
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMemoryCache();
builder.Services.AddRouting();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IBankService, BankService>();
builder.Services.AddScoped<IDues, DueService>();
builder.Services.AddAuthentication().AddScheme<ApiKeySchemaOptions, ApiKeyHandler>(ApiKeySchemaOptions.DefaultSchema, _ => { });
builder.Services.AddHealthChecks().AddTypeActivatedCheck<DataBaseHealthCheck>("Database health check", args: new object[] { logger });
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader() ));

builder.WebHost.ConfigureKestrel((_, server) => {
    string portNumber = Environment.GetEnvironmentVariable("PORT") ?? "3000";
    int port = int.Parse(portNumber);
    server.Listen(IPAddress.Any, port);
});
builder.WebHost.UseKestrel(options => options.AddServerHeader = false);

WebApplication app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();
app.MapHealthChecks("/health", new HealthCheckOptions () {
    ResultStatusCodes = {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status500InternalServerError,
    },
});
app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseMiddleware<RouteNotFoundMiddleware>();
app.UseCors();
app.MapControllers();
app.Run();

public partial class Program { }