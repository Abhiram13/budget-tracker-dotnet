using System.Net;
using Application;
using Services;
using Defination;

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
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IBankService, BankService>();
builder.WebHost.ConfigureKestrel((context, server) => {
    string portNumber = Environment.GetEnvironmentVariable("PORT") ?? "3000";
    int PORT = int.Parse(portNumber);
    server.Listen(IPAddress.Any, PORT);
});

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
