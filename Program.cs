using System.Net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using BudgetTracker.Services;
using BudgetTracker.Injectors;
using BudgetTracker.Defination;
using BudgetTracker.Application;
using Google.Cloud.Diagnostics.AspNetCore3;
using Google.Cloud.Diagnostics.Common;
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
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMemoryCache();
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
app.UseCors();
app.MapGet("/", async context => {
    try
    {
        using (IAsyncCursor<string>? collections = await Mongo.DB.ListCollectionNamesAsync())
        {
            List<string>? list = await collections.ToListAsync();

            if (list?.Count > 0)
            {
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                return;
            }

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }
    }
    catch (TypeInitializationException e)
    {
        logger?.Log(LogLevel.Critical, e, "Exception at PING API");
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
    }
    catch (Exception e)
    {
        logger?.Log(LogLevel.Critical, e, "Exception at PING API");
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
    }
});
app.MapControllers();
app.UseStatusCodePages(async context =>
{
    if (context.HttpContext.Response.StatusCode == 404)
    {
        ApiResponse<string> response = new ApiResponse<string>
        {
            StatusCode = HttpStatusCode.NotFound,
            Message = "Route not found",
        };
        await CustomResponse.Send(context.HttpContext, response);
    }
});
app.Run();