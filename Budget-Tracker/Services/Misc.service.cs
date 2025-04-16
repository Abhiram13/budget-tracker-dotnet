using BudgetTracker.Interface;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;

namespace BudgetTracker.Services
{
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