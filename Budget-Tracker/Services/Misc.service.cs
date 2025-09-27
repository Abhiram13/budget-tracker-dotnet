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
        private readonly IMongoContext _mongoContext;
        private readonly ILogger<DataBaseHealthCheck> _logger;

        public DataBaseHealthCheck(IMongoContext context, ILogger<DataBaseHealthCheck> logger)
        {
            _mongoContext = context;
            _logger = logger;
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
            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"EXCEPTION: At Database health check. Message ({0})", ex.Message);
                return HealthCheckResult.Degraded();
            }            
        }
    }
}