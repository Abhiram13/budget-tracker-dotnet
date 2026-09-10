using System.Net;
using Abhiram.Abstractions.Logging;
using Abhiram.Extensions.DotEnv;
using BudgetTracker.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
DotEnvironmentVariables.Load();

builder.AddConsoleGoogleSeriLog();
builder.Services.AddServiceCollection();

builder.WebHost.ConfigureKestrel((_, server) => {
    string portNumber = Environment.GetEnvironmentVariable("PORT") ?? "3000";
    int port = int.Parse(portNumber);
    server.Listen(IPAddress.Any, port);
});

WebApplication app = builder.Build();

app.UseApplicationServices();
app.Run();

public partial class Program { }