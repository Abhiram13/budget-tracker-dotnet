using System.Net;
using Abhiram.Abstractions.Logging;
using Abhiram.Extensions.DotEnv;
using Abhiram.Secrets.Providers;
using Abhiram.Secrets.Providers.Interface;
using BudgetTracker.Api.Middlewares;
using BudgetTracker.Api.Workers;
using BudgetTracker.Core.Application.Interfaces;
using BudgetTracker.Core.Application.Services;
using BudgetTracker.Core.Domain.ValueObject;
using BudgetTracker.Infrastructure.Persistence;
using BudgetTracker.Infrastructure.Repository;
using BudgetTracker.Infrastructure.Security;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Diagnostics.HealthChecks;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
DotEnvironmentVariables.Load();

builder.AddConsoleGoogleSeriLog();
builder.Services.AddRouting();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<TransactionService>();
builder.Services.AddScoped<BankService>();
builder.Services.AddSingleton<ICategoryRepository, CategoryRepository>();
builder.Services.AddSingleton<IBankRepository, BankRepository>();
builder.Services.AddSingleton<ITransactionRepository, TransactionRepository>();
builder.Services.AddSingleton<IMongoContext, MongoDBContext>();
builder.Services.AddSingleton<ISecretManager, SecretManagerService>();
builder.Services.AddSingleton<AppSecrets>();
builder.Services.AddHostedService<SecretHostService>();
builder.Services.AddAuthentication().AddScheme<ApiKeySchemaOptions, ApiKeyHandler>(ApiKeySchemaOptions.DefaultSchema, _ => { });

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();
builder.Services.AddRouting();
builder.Services.AddHealthChecks();
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

builder.WebHost.ConfigureKestrel((_, server) => {
    string portNumber = Environment.GetEnvironmentVariable("PORT") ?? "3000";
    int port = int.Parse(portNumber);
    server.Listen(IPAddress.Any, port);
});

WebApplication app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapHealthChecks("/health", new HealthCheckOptions () {
    ResultStatusCodes = {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status500InternalServerError,
    },
});
app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseCors();
app.MapControllers();
app.Run();

public partial class Program { }