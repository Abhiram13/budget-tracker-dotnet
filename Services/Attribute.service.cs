using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using BudgetTracker.Services;
using BudgetTracker.Defination;
using BudgetTracker.Security.Jwt;

namespace BudgetTracker.Attributes
{
    public class Authorization : ActionFilterAttribute
    {
        public override async void OnActionExecuting(ActionExecutingContext context)
        {
            try
            {
                string header = context.HttpContext.Request.Headers.Authorization.ToString();
                string token = FetchBearerToken(header);
                Payload? payload = Service.Decode(token);
                UserService service = new UserService();
                User? user = await service.FetchByIdAndName(payload?.UserId ?? "", payload?.Username ?? "");

                if (string.IsNullOrEmpty(user?.Id))
                {
                    ApiResponse<string> response = new()
                    {
                        Message = "Invalid token provided",
                        StatusCode = System.Net.HttpStatusCode.Unauthorized
                    };

                    context.Result = new UnauthorizedObjectResult(response);
                }

                base.OnActionExecuting(context);
            }
            catch (Exception)
            {
                ApiResponse<string> response = new()
                {
                    Message = "Invalid token provided",
                    StatusCode = System.Net.HttpStatusCode.Unauthorized
                };

                context.Result = new UnauthorizedObjectResult(response);
            }
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