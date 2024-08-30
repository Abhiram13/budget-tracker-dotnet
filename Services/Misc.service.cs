using System.Net;
using BudgetTracker.Defination;

namespace BudgetTracker.Services
{
    public delegate ApiResponse<T> Callback<T>() where T : class;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">
    ///     The Type of result to the client in ApiResponse Object
    /// </typeparam>
    /// <returns></returns>
    public delegate Task<ApiResponse<T>> AsyncCallback<T>() where T : class;

    public static class Handler<T> where T : class
    {
        public static ApiResponse<T> Exception(Callback<T> callback)
        {
            try
            {
                return callback();
            }
            catch (BadRequestException e)
            {
                return new ApiResponse<T>()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = $"Bad request. Message: {e.Message}",
                };
            }
            catch (Exception e)
            {
                return new ApiResponse<T>()
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = $"Something went wrong. Message: {e.Message}",
                };
            }
        }

        public static async Task<ApiResponse<T>> Exception(AsyncCallback<T> callback, ILogger logger)
        {
            try
            {
                return await callback();
            }
            catch (BadRequestException e)
            {
                logger?.Log(LogLevel.Error, e, "Bad request exception at controller callback");
                return new ApiResponse<T>()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = $"Bad request. Message: {e.Message}",
                };
            }
            catch (OperationCanceledException e)
            {
                logger?.Log(LogLevel.Error, e, "Operation cancellation request exception at controller callback");
                return new ApiResponse<T>()
                {
                    StatusCode = HttpStatusCode.Conflict,
                    Message = $"Operation cancellation request. Message: {e.Message}",
                };
            }
            catch (Exception e)
            {
                logger?.Log(LogLevel.Error, e, "Exception at controller callback");
                return new ApiResponse<T>()
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = $"Something went wrong. Message: {e.Message}",
                };
            }
        }
    }

    public class BadRequestException : Exception
    {
        public BadRequestException(string message) : base(message) { }
    }
}