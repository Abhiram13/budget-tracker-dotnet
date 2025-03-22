using System.Net;
using BudgetTracker.Defination;
using BudgetTracker.Interface;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;

namespace BudgetTracker.Services
{
    [Obsolete]
    public delegate ApiResponse<T> Callback<T>() where T : class;
    
    [Obsolete]
    public delegate Task<ApiResponse<T>> AsyncCallback<T>() where T : class;

    [Obsolete]
    public static class Handler<T> where T : class
    {
        public static ApiResponse<T> Exception(Callback<T> callback)
        {
            try
            {
                return callback();
            }
            catch (BadRequestException e)
            {
                return new ApiResponse<T>()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = $"Bad request. Message: {e.Message}",
                };
            }
            catch (Exception e)
            {
                return new ApiResponse<T>()
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = $"Something went wrong. Message: {e.Message}",
                };
            }
        }

        public static async Task<ApiResponse<T>> Exception(AsyncCallback<T> callback, ILogger logger)
        {
            try
            {
                return await callback();
            }
            catch (BadRequestException e)
            {
                logger?.Log(LogLevel.Error, e, "Bad request exception at controller callback");
                return new ApiResponse<T>()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = $"Bad request. Message: {e.Message}",
                };
            }
            catch (OperationCanceledException e)
            {
                logger?.Log(LogLevel.Error, e, "Operation cancellation request exception at controller callback");
                return new ApiResponse<T>()
                {
                    StatusCode = HttpStatusCode.Conflict,
                    Message = $"Operation cancellation request. Message: {e.Message}",
                };
            }
            catch (Exception e)
            {
                logger?.Log(LogLevel.Error, e, "Exception at controller callback");
                return new ApiResponse<T>()
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = $"Something went wrong. Message: {e.Message}",
                };
            }
        }
    }

    /// <summary>
    /// Throws when the request payload contains invalid values for any given property
    /// </summary>
    public class BadRequestException : Exception
    {
        public BadRequestException() : base ("Invalid data provided") { }

        public BadRequestException(string message) : base(message) { }
    }

    public class DataBaseHealthCheck : IHealthCheck
    {
        private readonly ILogger? _logger;
        private readonly IMongoContext _mongoContext;

        public DataBaseHealthCheck(ILogger? logger, IMongoContext context)
        {
            _logger = logger;
            _mongoContext = context;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using (IAsyncCursor<string>? collections = await _mongoContext.Database.ListCollectionNamesAsync(cancellationToken: cancellationToken))
                {
                    List<string>? list = await collections.ToListAsync(cancellationToken: cancellationToken);

                    if (list?.Count > 0)
                    {
                        return HealthCheckResult.Healthy();
                    }

                    return HealthCheckResult.Unhealthy();
                }
            }
            catch (TypeInitializationException e)
            {
                _logger?.Log(LogLevel.Critical, e, "Exception at PING API");
                return HealthCheckResult.Degraded();
            }
            catch (Exception e)
            {
                _logger?.Log(LogLevel.Critical, e, "Exception at PING API");
                return HealthCheckResult.Degraded();
            }            
        }
    }
}