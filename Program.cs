using System.Net;
using Application;
using Services;
using Defination;
using System.Text.Json;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string root = Directory.GetCurrentDirectory();
string dotenv = Path.Combine(root, ".env");
DotEnv.Load(dotenv);

string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// Add services to the container.
builder.Configuration.AddEnvironmentVariables().Build();
builder.Services.AddSingleton(s => Mongo.DB);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IBankService, BankService>();
builder.WebHost.ConfigureKestrel((context, server) => {
    string portNumber = Environment.GetEnvironmentVariable("PORT") ?? "3000";
    int PORT = int.Parse(portNumber);
    server.Listen(IPAddress.Any, PORT);
});

builder.Services.AddCors(options =>
{
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
app.UseStatusCodePages(async context => {
    if (context.HttpContext.Response.StatusCode == 404)
    {
        byte[] bytes = ConvertObjectToByte(new ApiResponse<string> {
            StatusCode = HttpStatusCode.NotFound,
            Message = "Route not found",
        });

        context.HttpContext.Response.Headers.ContentType = "application/json";
        await context.HttpContext.Response.Body.WriteAsync(bytes, 0, bytes.Length);
    }
});
app.Run();

byte[] ConvertObjectToByte(object obj)
{
    byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(obj);
    return bytes;
}
