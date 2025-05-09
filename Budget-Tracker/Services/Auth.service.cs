using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace BudgetTracker.Security
{
    namespace Authentication
    {
        public class ApiKeySchemaOptions : AuthenticationSchemeOptions
        {
            public const string DefaultSchema = "ApiKeySchema";
            public const string HeaderName = "API_KEY";
        }

        public class ApiKeyHandler : AuthenticationHandler<ApiKeySchemaOptions>
        {
            public ApiKeyHandler(
                IOptionsMonitor<ApiKeySchemaOptions> options, 
                ILoggerFactory logger, 
                UrlEncoder encoder
            ) : base(options, logger, encoder) { }

            protected override Task<AuthenticateResult> HandleAuthenticateAsync()
            {
                bool isHeaderExist = Request.Headers.ContainsKey(ApiKeySchemaOptions.HeaderName);

                if (!isHeaderExist) 
                {              
                    return Task.FromResult(AuthenticateResult.Fail("No Key"));
                }

                string? HEADER_API_KEY = Request.Headers[ApiKeySchemaOptions.HeaderName];
                string? API_KEY = Environment.GetEnvironmentVariable("API_KEY");

                if (HEADER_API_KEY != API_KEY)
                {
                    return Task.FromResult(AuthenticateResult.Fail("Invalid key provided"));
                }

                List<Claim> claims = new ()
                {
                    new (ClaimTypes.Name, "Api User")
                };

                ClaimsIdentity identity = new (claims, Scheme.Name);
                ClaimsPrincipal principal = new (identity);
                AuthenticationTicket ticket = new AuthenticationTicket(principal, Scheme.Name);

                return Task.FromResult(AuthenticateResult.Success(ticket));
            }
        }
    }
}