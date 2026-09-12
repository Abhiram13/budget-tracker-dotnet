using System.Net;
using System.Text.Json;
using Abhiram.Abstractions.Logging;
using Abhiram.Extensions.DotEnv;
using BudgetTracker.Extensions;
using BudgetTracker.Models;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
DotEnvironmentVariables.Load();

builder.Configuration.AddJsonFile(Path.Combine("/Volumes/SecureDisk/BudgetTracker-Mongo/", "secrets.json"), optional: false, reloadOnChange: true);

builder.AddConsoleGoogleSeriLog();
builder.Services.AddServiceCollection(builder.Configuration);

builder.WebHost.ConfigureKestrel((context, server) =>
{
    AppSecrets? secret = context.Configuration.Get<AppSecrets>();
    string portNumber = secret?.Port ?? "3000";
    int port = int.Parse(portNumber);
    server.ListenAnyIP(port);
});

WebApplication app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
    MongoSecrets ms = scope.ServiceProvider.GetRequiredService<MongoSecrets>();
    ILogger<Program> logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    logger.LogCritical(JsonSerializer.Serialize(ms));
}

app.UseApplicationServices();
app.Run();

public partial class Program { }