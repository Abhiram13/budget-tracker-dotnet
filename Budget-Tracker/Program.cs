using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using BudgetTracker.Services;
using BudgetTracker.Interface;
using BudgetTracker.Defination;
using BudgetTracker.Middlewares;
using BudgetTracker.Security.Authentication;
using Abhiram.Abstractions.Logging;
using Abhiram.Extensions.DotEnv;
using Abhiram.Secrets.Providers;
using Abhiram.Secrets.Providers.Interface;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
DotEnvironmentVariables.Load();

builder.AddConsoleGoogleSeriLog();
builder.Services.AddSingleton<IMongoContext, MongoDBContext>();
builder.Services.AddScoped<ISecretManager, SecretManagerService>();
builder.Services.AddSingleton<AppSecrets>();
builder.Services.AddHostedService<SecretHostService>();
builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
{
    options.SuppressModelStateInvalidFilter = false;
    options.InvalidModelStateResponseFactory = action =>
    {
        KeyValuePair<string, ModelStateEntry?> modelState = action.ModelState.FirstOrDefault();
        string errorAt = modelState.Key;
        string errorMessage = modelState.Value?.Errors?[0].ErrorMessage ?? $"Something went wrong at {errorAt}";
        return new BadRequestObjectResult(new ApiResponse<string> { Message = errorMessage, StatusCode = HttpStatusCode.BadRequest });
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
builder.Services.AddHealthChecks().AddTypeActivatedCheck<DataBaseHealthCheck>("Database health check", args: new object[] {  });
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