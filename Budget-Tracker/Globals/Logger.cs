// using System.Text.Json;

// namespace BudgetTracker.Application;

// public static class Logger
// {
//     public static void Log<T>(T arg)
//     {
//         Console.WriteLine(JsonSerializer.Serialize(arg));
//     }

//     private static ILogger? _loggerInstance;
//     private static readonly object _lock = new object();

//     /// <summary>
//     /// Initializes the static logger instance.
//     /// This should be called once early in your application's startup.
//     /// </summary>
//     /// <param name="logger">The ILogger instance to use.</param>
//     public static void Initialize(ILogger logger)
//     {
//         lock (_lock)
//         {
//             if (_loggerInstance != null)
//             {
//                 // Optionally log a warning if Initialize is called more than once
//                 _loggerInstance.LogWarning("Logger.Initialize called more than once.");
//                 return;
//             }
//             _loggerInstance = logger;
//         }
//     }

//     /// <summary>
//     /// Logs an informational message.
//     /// </summary>
//     /// <param name="message">The message to log.</param>
//     /// <param name="args">Optional arguments for the message.</param>
//     public static void LogInformation<T>(T message, params object[] args)
//     {
//         string payload = JsonSerializer.Serialize(message);
//         _loggerInstance?.LogInformation(payload, args);
//     }

//     /// <summary>
//     /// Logs a warning message.
//     /// </summary>
//     /// <param name="message">The message to log.</param>
//     /// <param name="args">Optional arguments for the message.</param>
//     public static void LogWarning<T>(T message, params object[] args)
//     {
//         string payload = JsonSerializer.Serialize(message);
//         _loggerInstance?.LogWarning(payload, args);
//     }

//     /// <summary>
//     /// Logs an error message.
//     /// </summary>
//     /// <param name="message">The message to log.</param>
//     /// <param name="exception">An optional exception to log.</param>
//     /// <param name="args">Optional arguments for the message.</param>
//     public static void LogError<T>(Exception? exception, T message, params object[] args)
//     {
//         string payload = JsonSerializer.Serialize(message);
//         _loggerInstance?.LogError(exception, payload, args);
//     }

//     /// <summary>
//     /// Logs a critical message.
//     /// </summary>
//     /// <param name="message">The message to log.</param>
//     /// <param name="exception">An optional exception to log.</param>
//     /// <param name="logDetails">A dictionary containing key-value pairs of request and contextual information to be included in the log scope.</param>
//     /// <param name="args">Optional arguments for the message.</param>
//     public static void LogCritical<T>(Exception? exception, T message, params object[] args)
//     {
//         string payload = JsonSerializer.Serialize(message);
//         _loggerInstance?.LogCritical(exception, payload, args);
//     }
    
//     /// <summary>
//     /// Logs a Debug message.
//     /// </summary>
//     /// <param name="message">The message to log.</param>
//     /// <param name="args">Optional arguments for the message.</param>
//     public static void LogDebug<T>(T message, params object[] args)
//     {
//         string payload = JsonSerializer.Serialize(message);
//         _loggerInstance?.LogDebug(payload, args);
//     }
// }