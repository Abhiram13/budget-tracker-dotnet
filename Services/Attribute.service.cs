using Defination;
using Global;
using JWT;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Services;

namespace budget_tracker.Attributes
{    
    public class JwtAuthourise : ActionFilterAttribute
    {
        public override async void OnActionExecuting(ActionExecutingContext context)
        {
            string header = context.HttpContext.Request.Headers.Authorization.ToString();
            string token = FetchBearerToken(header);
            Payload? payload = Service.Decode(token);
            UserService service = new UserService();
            User? user = await service.FetchByIdAndName(payload?.UserId ?? "", payload?.Username ?? "");

            if (string.IsNullOrEmpty(user?.Id))
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