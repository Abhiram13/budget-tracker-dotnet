using System.Net;
using Application;
using Services;
using Defination;
using Global;
using MongoDB.Driver;
using Google.Cloud.Diagnostics.AspNetCore3;
using Google.Cloud.Diagnostics.Common;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string root = Directory.GetCurrentDirectory();
string dotenv = Path.Combine(root, ".env");
DotEnv.Load(dotenv);

using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddGoogle());
ILogger logger = factory.CreateLogger("Program");

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
builder.Logging.ClearProviders();
builder.Services.AddGoogleDiagnosticsForAspNetCore();
builder.WebHost.ConfigureKestrel((context, server) => {
    string portNumber = Environment.GetEnvironmentVariable("PORT") ?? "3000";
    int PORT = int.Parse(portNumber);
    server.Listen(IPAddress.Any, PORT);
});

builder.WebHost.UseKestrel(options => options.AddServerHeader = false);
builder.Services.AddCors(options => {
    options.AddDefaultPolicy(builder => {
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.Use(async (context, next) => {    
    context.Response.Headers.ContentType = "application/json";
    await next.Invoke();    
});
app.UseCors();
app.MapGet("/", async context => {
    try
    {
        logger.LogInformation("This is Sample Info from ASP.NET Core Google");
        logger.LogError("This is Sample Error from ASP.NET Core Google");
        logger.LogCritical("This is Sample Critical from ASP.NET Core Google");
        logger.LogDebug("This is Sample Debug from ASP.NET Core Google");
        logger.LogTrace("This is Sample Trace from ASP.NET Core Google");
        logger.LogWarning("This is Sample Warning from ASP.NET Core Google");
        
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
    catch (Exception)
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
    }    
});
app.MapControllers();
app.UseStatusCodePages(async context => {
    if (context.HttpContext.Response.StatusCode == 404)
    {
        ApiResponse<string> response = new ApiResponse<string> {
            StatusCode = HttpStatusCode.NotFound,
            Message = "Route not found",
        };
        byte[] bytes = ResponseBytes.Convert(response);
        await context.HttpContext.Response.Body.WriteAsync(bytes, 0, bytes.Length);
    }
});
app.Run();