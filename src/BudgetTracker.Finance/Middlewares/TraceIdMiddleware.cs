using BudgetTracker.Core.Application.Interfaces;

namespace BudgetTracker.Api.Middlewares;

public sealed class TraceIdMiddleware : ICustomMiddleware
{
    private readonly RequestDelegate _requestDelegate;
    private readonly ILogger<TraceIdMiddleware> _logger;

    public TraceIdMiddleware(RequestDelegate requestDelegate, ILogger<TraceIdMiddleware> logger)
    {
        _requestDelegate = requestDelegate;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        string traceId = Guid.NewGuid().ToString();
        using (_logger.BeginScope(new Dictionary<string, object> { ["trace_id"] = traceId }))
        {
            await _requestDelegate(httpContext);
        }
    }
}