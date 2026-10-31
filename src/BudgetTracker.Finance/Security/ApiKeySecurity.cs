using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using BudgetTracker.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BudgetTracker.Security;

public class ApiKeySchemaOptions : AuthenticationSchemeOptions
{
    public const string DEFAULT_SCHEMA = "ApiKeySchema";
    public const string HEADER_NAME = "API_KEY";
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
        bool isHeaderExist = Request.Headers.ContainsKey(ApiKeySchemaOptions.HEADER_NAME);

        if (!isHeaderExist)
        {
            return Task.FromResult(AuthenticateResult.Fail("No Key"));
        }

        string? headerApiKey = Request.Headers[ApiKeySchemaOptions.HEADER_NAME];
        string? apiKey = _secret.Apikey;

        if (headerApiKey != apiKey)
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid key provided"));
        }

        List<Claim> claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "Api User")
        };

        ClaimsIdentity identity = new ClaimsIdentity(claims, Scheme.Name);
        ClaimsPrincipal principal = new(identity);
        AuthenticationTicket ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
