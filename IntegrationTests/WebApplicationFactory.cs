using Abhiram.Secrets.Configuration;
using BudgetTracker.Context;
using BudgetTracker.Interfaces;
using BudgetTracker.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace IntegrationTests.Utils;

public class FinanceWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: false, reloadOnChange: true)
                .AddSecrets(environment: context.HostingEnvironment, optional: false)
                .AddEnvironmentVariables();
        });

        builder.ConfigureServices((_, services) =>
        {
            ServiceDescriptor secretsDescriptor = services.Single(s => s.ServiceType == typeof(MongoSecrets));
            ServiceDescriptor mongoContextDescriptor = services.Single(s => s.ServiceType == typeof(IMongoContext));

            services.Remove(secretsDescriptor);
            services.Remove(mongoContextDescriptor);
            
            services.AddOptions<MongoSecrets>().BindConfiguration("Mongo").ValidateOnStart();

            services.AddSingleton<IMongoContext, MongoDBContext>(s =>
            {
                MongoSecrets secrets = s.GetRequiredService<IOptions<MongoSecrets>>().Value;
                MongoDBContext context = new MongoDBContext(secrets);

                return context;
            });
        });
        
        base.ConfigureWebHost(builder);
    }
}