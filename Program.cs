using System.Net;
using Application;
using Services;
using Defination;
using System.Text.Json;

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
builder.Services.AddScoped<IUserService, UserService>();
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
    // var test = JwtService.Decode("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1lIjoiQWJoaXJhbSIsIm5iZiI6MTcxODcxNjkwMiwiZXhwIjoxNzE4NzE3NTAyLCJpYXQiOjE3MTg3MTY5MDIsImlzcyI6Ik5hZ2FkaSJ9.FZ6Iyrvrm0BrWNWzj8v8bQhAMQQfrUMLYIJb8hseRuY");
    // Console.WriteLine(test?.Name);

    Security.Hash.GenerateHashedPassword("13@Kadapa");
    await next.Invoke();
});
app.Run();
