using Google.Api;
using Google.Cloud.Logging.Type;
using Google.Cloud.Logging.V2;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json;

namespace BudgetTracker.Application;

public static class LoggingExtensions
{
    private static ILoggerFactory? factory;
    private static ILogger? logger;
    
    public static WebApplicationBuilder AddLogger(this WebApplicationBuilder builder)
    {
        if (Environment.GetEnvironmentVariable("ENV") == "Development" || Environment.GetEnvironmentVariable("ENV") == "Test")
        {
            AddConsoleLogger(builder);
        }
        else
        {

        }
        return builder;
    }

    private static void AddConsoleLogger(WebApplicationBuilder builder)
    {
        builder.Logging.AddConsole();
        factory = LoggerFactory.Create(log =>
        {
            log.AddSimpleConsole(options =>
            {
                options.IncludeScopes = true;
                options.SingleLine = true;
                options.TimestampFormat = "HH:mm:ss ";
                options.IncludeScopes = true;
            });
        });
        logger = factory.CreateLogger("Budget-tracker-console");
        Logger.Initialize(logger);
    }

    private static void AddGoogleLogger(object data, LogSeverity severity, string message)
    {
        LoggingServiceV2Client client = LoggingServiceV2Client.Create();
        string projectId = "budget-tracker-453204";
        LogName logName = new LogName(projectId, "budget-tracker-logs");
        MonitoredResource resource = new MonitoredResource { Type = "global" };
        resource.Labels.Add("project_id", projectId);        

        // 1. Serialize the C# object to a JSON string
        var jsonString = JsonConvert.SerializeObject(data);

        // 2. Parse the JSON string into a Protobuf Struct
        var jsonStruct = JsonParser.Default.Parse<Struct>(jsonString);

        // 3. Create the LogEntry
        var logEntry = new LogEntry
        {
            LogNameAsLogName = logName,
            Severity = severity,
            // The main log message, visible in the summary
            TextPayload = message,
            // The structured data payload
            JsonPayload = jsonStruct,
            Resource = resource 
        };

        // You can also add simple key-value pairs using labels
        logEntry.Labels.Add("userId", "12345");
        logEntry.Labels.Add("environment", "production");

        // 4. Send the log entry to Google Cloud Logging
        client.WriteLogEntries(logName, null, null, new[] { logEntry });
    }
}