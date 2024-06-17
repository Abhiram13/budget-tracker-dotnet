using System.Net;
using Application;
using Services;
using Defination;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication;
using MongoDB.Bson;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string root = Directory.GetCurrentDirectory();
string dotenv = Path.Combine(root, ".env");
DotEnv.Load(dotenv);

// Add services to the container.
builder.Configuration.AddEnvironmentVariables().Build();
builder.Services.AddSingleton(s => Mongo.DB);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();
builder.Services.AddTransient<JwtMiddleware>();
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
app.UseCors();
app.UseAuthorization();
app.MapControllers();
app.UseMiddleware<JwtMiddleware>();
app.UseStatusCodePages(async context => {
    context.HttpContext.Response.Headers.ContentType = "application/json";

    Func<object, byte[]> ConvertObjToBytes = (object obj) => {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(obj);
        return bytes;
    };

    if (context.HttpContext.Response.StatusCode == 404)
    {
        byte[] bytes = ConvertObjToBytes(new ApiResponse<string> {
            StatusCode = HttpStatusCode.NotFound,
            Message = "Route not found",
        });        
        await context.HttpContext.Response.Body.WriteAsync(bytes, 0, bytes.Length);
    }
});
app.Use(async (context, next) => {
    string token = Jwt<string>.Create();
    // Console.WriteLine(token);
    // string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IkFiaGlyYW0iLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9hbm9ueW1vdXMiOiIyMyIsIm5iZiI6MTcxODYzNzMxOSwiZXhwIjoxNzE4NjM3OTE5LCJpYXQiOjE3MTg2MzczMTl9.gPROLEup9iV1jwLY72UY6sqLUzPRpqZKx7D6EcQ9x9Q";
    JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
    // SecurityToken readToken = handler.ReadJwtToken(token);        
    string[]? split = handler?.ReadToken(token)?.ToString()?.Split(".");

    // string json = readToken
    // Console.WriteLine(json);
    Defination.JwtPayload? jwt = JsonSerializer.Deserialize<Defination.JwtPayload>(split?[1]);    
    Console.WriteLine(jwt?.Iss);
    await next.Invoke();
});
app.Run();
