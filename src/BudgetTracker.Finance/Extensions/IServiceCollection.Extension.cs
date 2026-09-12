using System.Net;
using Abhiram.Secrets.Providers;
using Abhiram.Secrets.Providers.Interface;
using BudgetTracker.Workers;
using BudgetTracker.Interfaces;
using BudgetTracker.Services;
using BudgetTracker.ValueObject;
using BudgetTracker.Context;
using BudgetTracker.Models;
using BudgetTracker.Repository;
using BudgetTracker.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;

namespace BudgetTracker.Extensions;

public static class ServiceCollectionExtension
{
    private static IConfiguration _configuration = default!;
    
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection AddServiceCollection(IConfiguration configuration)
        {
            _configuration = configuration;
            
            serviceCollection
                .AddMongoSecrets()
                .AddDependencyServices()
                .AddControllerConfiguration()
                .AddAuthConfiguration()
                .AddApplicationServices();

            return serviceCollection;
        }
        
        private IServiceCollection AddDependencyServices()
        {
            serviceCollection
                .AddSingleton<MongoSecrets>(provider => provider.GetRequiredService<IOptions<MongoSecrets>>().Value)
                .AddSingleton<ICategoryRepository, CategoryRepository>()
                .AddSingleton<IBankRepository, BankRepository>()
                .AddSingleton<ITransactionRepository, TransactionRepository>()
                .AddSingleton<IDueRepository, DueRepository>()
                .AddSingleton<IMongoContext, MongoDBContext>()
                .AddScoped<CategoryService>()
                .AddScoped<TransactionService>()
                .AddScoped<BankService>()
                .AddScoped<DueService>()
                .AddSingleton<AppSecrets>()
                .AddSingleton<ISecretManager, SecretManagerService>();

            return serviceCollection;
        }

        private IServiceCollection AddControllerConfiguration()
        {
            serviceCollection.AddControllers().ConfigureApiBehaviorOptions(options =>
            {
                options.SuppressModelStateInvalidFilter = false;
                options.InvalidModelStateResponseFactory = action =>
                {
                    KeyValuePair<string, ModelStateEntry?> modelState = action.ModelState.FirstOrDefault();
                    string errorAt = modelState.Key;
                    string errorMessage = modelState.Value?.Errors?[0].ErrorMessage ?? $"Something went wrong at {errorAt}";
                    return new BadRequestObjectResult(new ApiResponse<string> { Message = errorMessage, StatusCode = HttpStatusCode.BadRequest });
                };
            });

            return serviceCollection;
        }

        private IServiceCollection AddAuthConfiguration()
        {
            serviceCollection
                .AddAuthentication()
                .AddScheme<ApiKeySchemaOptions, ApiKeyHandler>(ApiKeySchemaOptions.DefaultSchema, _ => { });

            return serviceCollection;
        }

        private IServiceCollection AddApplicationServices()
        {
            serviceCollection
                .AddRouting()
                .AddEndpointsApiExplorer()
                .AddMemoryCache()
                .AddCors(opt => opt.AddDefaultPolicy(pol => pol.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()))
                .AddHealthChecks();

            return serviceCollection;
        }

        private IServiceCollection AddMongoSecrets()
        {
            serviceCollection
                .AddOptions<MongoSecrets>()
                .BindConfiguration("Mongo")
                .ValidateDataAnnotations()
                .ValidateOnStart();
            
            return serviceCollection;
        }

        private IServiceCollection AddAppSecrets()
        {
            serviceCollection.AddOptions<AppSecrets>().Bind(_configuration).ValidateOnStart();
            
            return serviceCollection;
        }
    }
}