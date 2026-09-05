using System.Net;
using Abhiram.Secrets.Providers;
using Abhiram.Secrets.Providers.Interface;
using BudgetTracker.Api.Middlewares;
using BudgetTracker.Api.Workers;
using BudgetTracker.Core.Application.Interfaces;
using BudgetTracker.Core.Application.Services;
using BudgetTracker.Core.Domain.ValueObject;
using BudgetTracker.Infrastructure.Persistence;
using BudgetTracker.Infrastructure.Repository;
using BudgetTracker.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BudgetTracker.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDependencyServices(this IServiceCollection services)
    {
        services.AddScoped<CategoryService>();
        services.AddScoped<TransactionService>();
        services.AddScoped<BankService>();
        services.AddScoped<DueService>();
        services.AddSingleton<AppSecrets>();
        services.AddSingleton<ICategoryRepository, CategoryRepository>();
        services.AddSingleton<IBankRepository, BankRepository>();
        services.AddSingleton<ITransactionRepository, TransactionRepository>();
        services.AddSingleton<IDueRepository, DueRepository>();
        services.AddSingleton<IMongoContext, MongoDBContext>();
        services.AddSingleton<ISecretManager, SecretManagerService>();
        
        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddRouting();
        services.AddAuthentication().AddScheme<ApiKeySchemaOptions, ApiKeyHandler>(ApiKeySchemaOptions.DefaultSchema, _ => { });
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddMemoryCache();
        services.AddHealthChecks();
        services.AddCors(opt => opt.AddDefaultPolicy(pol => pol.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
        services.AddControllers().ConfigureApiBehaviorOptions(options =>
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
        return services;
    }

    public static IServiceCollection AddHostedServices(this IServiceCollection services)
    {
        services.AddHostedService<SecretHostService>();
        
        return services;
    }
}

public static class WebApplicationExtensions
{
    public static WebApplication UseMiddlewares(this WebApplication app)
    {
        app.UseMiddleware<ExceptionHandlerMiddleware>();
        app.UseMiddleware<TraceIdMiddleware>();
        
        return app;
    }

    public static WebApplication UseApplicationServices(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseHttpsRedirection();
        app.UseCors();
        app.MapControllers();
        
        return app;
    }
}