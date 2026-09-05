using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace BudgetTracker.Interfaces;

public interface ICustomMiddleware
{
    Task InvokeAsync(HttpContext httpContext);
}