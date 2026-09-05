using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace BudgetTracker.Core.Application.Interfaces;

public interface ICustomMiddleware
{
    Task InvokeAsync(HttpContext httpContext);
}