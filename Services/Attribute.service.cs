using Defination;
using Global;
using JWT;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace budget_tracker.Attributes
{    
    public class JwtAuthourise : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            string header = context.HttpContext.Request.Headers.Authorization.ToString();
            string token = FetchBearerToken(header);
            Payload? payload = Service.Decode(token);

            if (string.IsNullOrEmpty(payload?.Name))
            {
                ApiResponse<string> response = new() {
                    Message = "Invalid token provided",
                    StatusCode = System.Net.HttpStatusCode.Unauthorized
                };

                context.Result = new UnauthorizedObjectResult(response);
            }

            base.OnActionExecuting(context);
        }

        private string FetchBearerToken(string bearer)
        {            
            string[] split = bearer.Split(" ");
            string token = "";
            if (split.Length > 1)
            {
                token = split[1] ?? "";
            }

            return token;
        }
    }
}

[AttributeUsage(AttributeTargets.Property)]
public class ValidateAttribute : Attribute
{
    private string regex { get; set; }

    public ValidateAttribute(string _regex)
    {
        Console.WriteLine("Hey");
        regex = _regex;
        throw new Exception();
    }
}