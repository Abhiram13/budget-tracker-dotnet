using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using MongoDB.Driver;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using BudgetTracker.Services;
using BudgetTracker.Injectors;
using BudgetTracker.Defination;
using BudgetTracker.Application;
using Google.Cloud.Diagnostics.AspNetCore3;
using Google.Cloud.Diagnostics.Common;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;
using System.Text;
using Microsoft.IdentityModel.Tokens;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string root = Directory.GetCurrentDirectory();
string dotenv = Path.Combine(root, ".env");
DotEnv.Load(dotenv);

ILoggerFactory factory;
ILogger? logger = null;

// Add services to the container.
builder.Configuration.AddEnvironmentVariables().Build();
builder.Services.AddSingleton(s => Mongo.DB);
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options => {
        options.SuppressModelStateInvalidFilter = false;
        options.InvalidModelStateResponseFactory = action => {
            var modelState = action.ModelState.FirstOrDefault();
            string errorAt = modelState.Key;
            string errorMessage = modelState.Value?.Errors[0]?.ErrorMessage ?? $"Something went wrong at {errorAt}";
            return new BadRequestObjectResult(new ApiResponse<string> {Message = errorMessage, StatusCode = HttpStatusCode.BadRequest});
        };
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMemoryCache();
builder.Services.AddRouting();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IBankService, BankService>();
builder.Services.AddScoped<IDueService, DueService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.WebHost.ConfigureKestrel((context, server) => {
    string portNumber = Environment.GetEnvironmentVariable("PORT") ?? "3000";
    int PORT = int.Parse(portNumber);
    server.Listen(IPAddress.Any, PORT);
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => {
    string privateKey = "bAafd@A7d9#@F4*V!LHZs#ebKQrkE6pad2f3kj34c3dXy@";
    byte[] key = Encoding.UTF8.GetBytes(privateKey);

    options.IncludeErrorDetails = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidIssuer = "localhost.com",
        ValidAudience = "localhost.com",
        ValidateIssuer = true,
        ValidateIssuerSigningKey = true,
        ValidateAudience = true
    };
});

builder.Host.ConfigureLogging(logging => {
    if (Environment.GetEnvironmentVariable("ENV") == "Development")
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
app.UseStatusCodePagesWithReExecute("/error/{0}");
app.UseCors();
app.MapControllers();
app.Run();