using System.Net;
using Application;
using Services;
using Defination;
using Global;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string root = Directory.GetCurrentDirectory();
string dotenv = Path.Combine(root, ".env");
DotEnv.Load(dotenv);

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
// app.UseMiddleware<AuthenticationMiddleware>();
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