using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using BudgetTracker.Core.Domain.ValueObject;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BudgetTracker.Infrastructure.Security;

public class ApiKeySchemaOptions : AuthenticationSchemeOptions
{
    public const string DefaultSchema = "ApiKeySchema";
    public const string HeaderName = "API_KEY";
}

public class ApiKeyHandler : AuthenticationHandler<ApiKeySchemaOptions>
{
    private readonly AppSecrets _secret;

    public ApiKeyHandler(IOptionsMonitor<ApiKeySchemaOptions> options, ILoggerFactory logger, UrlEncoder encoder, AppSecrets secrets) : base(options, logger, encoder)
    {
        _secret = secrets;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        bool isHeaderExist = Request.Headers.ContainsKey(ApiKeySchemaOptions.HeaderName);

        if (!isHeaderExist)
        {
            return Task.FromResult(AuthenticateResult.Fail("No Key"));
        }

        string? HEADER_API_KEY = Request.Headers[ApiKeySchemaOptions.HeaderName];
        string? API_KEY = _secret.ApiKey;

        if (HEADER_API_KEY != API_KEY)
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid key provided"));
        }

        List<Claim> claims = new List<Claim>()
        {
            new Claim(ClaimTypes.Name, "Api User")
        };

        ClaimsIdentity identity = new(claims, Scheme.Name);
        ClaimsPrincipal principal = new(identity);
        AuthenticationTicket ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}