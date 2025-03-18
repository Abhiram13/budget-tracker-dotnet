using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using MongoDB.Driver;
using BudgetTracker.Services;
using BudgetTracker.Interface;
using BudgetTracker.Defination;
using BudgetTracker.Application;
using BudgetTracker.Middlewares;
using BudgetTracker.Security.Authentication;
using Google.Cloud.Diagnostics.AspNetCore3;
using Google.Cloud.Diagnostics.Common;
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Diagnostics.HealthChecks;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string root = Directory.GetCurrentDirectory();
string dotenv = Path.Combine(root, ".env");
DotEnv.Load(dotenv);

ILoggerFactory factory;
ILogger? logger = null;

// Add services to the container.
builder.Configuration.AddEnvironmentVariables().Build();
builder.Services.AddSingleton(_ => Mongo.DB);
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options => {
        options.SuppressModelStateInvalidFilter = false;
        options.InvalidModelStateResponseFactory = action => {
            KeyValuePair<string, ModelStateEntry?> modelState = action.ModelState.FirstOrDefault();
            string errorAt = modelState.Key;
            string errorMessage = modelState.Value?.Errors?[0]?.ErrorMessage ?? $"Something went wrong at {errorAt}";            
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
builder.WebHost.ConfigureKestrel((context, server) => {
    string portNumber = Environment.GetEnvironmentVariable("PORT") ?? "3000";
    int port = int.Parse(portNumber);
    server.Listen(IPAddress.Any, port);
});

builder.Services.AddAuthentication().AddScheme<ApiKeySchemaOptions, ApiKeyHandler>(ApiKeySchemaOptions.DefaultSchema, options => {});

builder.Host.ConfigureLogging(logging => {
    if (Environment.GetEnvironmentVariable("ENV") == "Development" || Environment.GetEnvironmentVariable("ENV") == "Test")
    {
        factory = LoggerFactory.Create(log => log.AddConsole());
        logger = factory.CreateLogger("Program");
        return;
    }

    factory = LoggerFactory.Create(log => log.AddGoogle());
    logger = factory.CreateLogger("Program");
    logging.Services.AddGoogleDiagnosticsForAspNetCore();
    return;
});
builder.Services.AddHealthChecks().AddTypeActivatedCheck<DataBaseHealthCheck>("Database health check", args: new object[] {logger!});
builder.WebHost.UseKestrel(options => options.AddServerHeader = false);
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

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