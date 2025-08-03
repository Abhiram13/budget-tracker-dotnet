using System.Security.Claims;
using System.Text.Encodings.Web;
using BudgetTracker.Application;
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
                
                Logger.LogInformation("This is just a log info from Authentication middleware");
                WriteLine("This is just a console info from Authentication middleware");

                if (!isHeaderExist)
                {
                    return Task.FromResult(AuthenticateResult.Fail("No Key"));
                }

                string? HEADER_API_KEY = Request.Headers[ApiKeySchemaOptions.HeaderName];
                // string? API_KEY = Environment.GetEnvironmentVariable("API_KEY");
                string? API_KEY = Secrets.API_KEY;

                Logger.LogInformation("Logger --> Header Key: {0}, API_KEY: {1}", HEADER_API_KEY, API_KEY);
                WriteLine("Console --> Header Key: {0}, API_KEY: {1}", HEADER_API_KEY, API_KEY);

                if (HEADER_API_KEY != API_KEY)
                {
                    return Task.FromResult(AuthenticateResult.Fail("Invalid key provided"));
                }

                List<Claim> claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.Name, "Api User")
                };

                ClaimsIdentity identity = new (claims, Scheme.Name);
                ClaimsPrincipal principal = new (identity);
                AuthenticationTicket ticket = new AuthenticationTicket(principal, Scheme.Name);

                return Task.FromResult(AuthenticateResult.Success(ticket));
            }
        }
    }
}