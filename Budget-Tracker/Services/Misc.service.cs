// using BudgetTracker.Application;
using BudgetTracker.Interface;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;
using CustomUtilities;

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

        public DataBaseHealthCheck(IMongoContext context)
        {
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
            // TODO: Update this duplicate exceptional messages
            catch (TypeInitializationException e)
            {
                Logger.LogCritical(e, "HEALTH CHECK: Exception at PING API");
                return HealthCheckResult.Degraded();
            }
            catch (Exception e)
            {
                Logger.LogCritical(e, "HEALTH CHECK: Exception at PING API");
                return HealthCheckResult.Degraded();
            }            
        }
    }
}