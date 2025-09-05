using Google.Cloud.Logging.V2;
using Google.Cloud.Logging.Type;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Google.Api;
using System.Text.Json;
using Google.Protobuf;
using System.Text.Json.Nodes;

namespace BudgetTracker.Application;

public class GoogleApiLogger : ILogger
{
    private readonly string _category;
    private readonly LoggingServiceV2Client _client;
    private readonly string _projectId;

    public GoogleApiLogger(string category, LoggingServiceV2Client client, string projectId)
    {
        _category = category;
        _client = client;
        _projectId = projectId;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel != LogLevel.None;
    }


    // public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception, string> formatter)
    // {
    //     if (!IsEnabled(logLevel)) return;

    //     var logName = new LogName(_projectId, _category);
    //     var payload = formatter(state, exception!);

    //     if (exception != null)
    //     {
    //         payload += $"\nException: {exception}";
    //     }

    //     var entry = new LogEntry
    //     {
    //         LogName = logName.ToString(),
    //         Severity = ConvertSeverity(logLevel),
    //         Timestamp = Timestamp.FromDateTime(DateTime.UtcNow),
    //         TextPayload = payload,
    //         Resource = new MonitoredResource { Type = "global" }
    //     };

    //     _client.WriteLogEntries(logName: logName, resource: entry.Resource, labels: null, entries: new[] { entry });
    // }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception, string> formatter)
    {
        string message = formatter(state, exception ?? new Exception());
        // IDictionary<string, object>? structuredData = null;
        LogName logName = new LogName(_projectId, "categoryName");

        // if (state is IEnumerable<KeyValuePair<string, object>> kvps)
        // {
        //     structuredData = kvps.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        // }

        // string jsonString = JsonSerializer.Serialize(state);
        // Struct jsonStruct = JsonParser.Default.Parse<Struct>(jsonString);

        var entry = new LogEntry
        {
            LogName = $"projects/{_projectId}/logs/categoryName",
            Severity = ConvertSeverity(logLevel),
            Timestamp = Timestamp.FromDateTime(DateTime.UtcNow),
            Resource = new MonitoredResource { Type = "cloud_run_revision" },
            // JsonPayload = jsonStruct,
            TextPayload = message
        };

        if (state is IEnumerable<KeyValuePair<string, object>> structured)
        {
            var jsonStruct = new Struct();

            foreach (var kvp in structured)
            {
                if (kvp.Value is null)
                {
                    jsonStruct.Fields[kvp.Key] = Value.ForNull();
                }
                else if (kvp.Value.GetType().IsPrimitive || kvp.Value is string)
                {
                    jsonStruct.Fields[kvp.Key] = Value.ForString(kvp.Value.ToString());
                }
                else
                {
                    // 🧠 ✅ STRUCTURED OBJECT INJECTION
                    jsonStruct.Fields[kvp.Key] = Value.ForStruct(ConvertObjectToStruct(kvp.Value));
                }
            }

            entry.JsonPayload = jsonStruct;
        }

        // if (structuredData != null)
        // {
        //     var jsonStruct = new Struct();
        //     foreach (var kvp in structuredData)
        //     {
        //         jsonStruct.Fields[kvp.Key] = Value.ForString(kvp.Value?.ToString() ?? "");
        //     }
        //     entry.JsonPayload = jsonStruct;
        // }
        // else
        // {
        //     entry.TextPayload = message;
        // }

        // if (state is IEnumerable<KeyValuePair<string, object>> structured)
        // {
        //     Struct jsonStruct = new Struct();

        //     foreach (var kvp in structured)
        //     {
        //         if (kvp.Value is null)
        //         {
        //             jsonStruct.Fields[kvp.Key] = Value.ForNull();
        //         }
        //         else if (kvp.Value.GetType().IsPrimitive || kvp.Value is string)
        //         {
        //             jsonStruct.Fields[kvp.Key] = Value.ForString(kvp.Value.ToString());
        //         }
        //         else
        //         {
        //             // 👇 Serialize complex object to JSON string
        //             string json = JsonSerializer.Serialize(kvp.Value);
        //             jsonStruct.Fields[kvp.Key] = Value.ForString(json);
        //         }
        //     }

        //     entry.JsonPayload = jsonStruct;
        //     // entry.JsonPayload = state;
        // }
        // else
        // {
        //     entry.TextPayload = message;
        // }

        _client.WriteLogEntries(logName: logName, resource: entry.Resource, labels: null, entries: new[] { entry });
    }

    private Struct ConvertObjectToStruct(object obj)
    {
        string? json = JsonSerializer.Serialize(obj);
        JsonNode? jsonNode = JsonNode.Parse(json);

        return ConvertJsonNodeToStruct(jsonNode!);
    }

    private Struct ConvertJsonNodeToStruct(JsonNode node)
    {
        Struct result = new Struct();
        if (node is JsonObject obj)
        {
            foreach (var prop in obj)
            {
                result.Fields[prop.Key] = ConvertJsonNodeToValue(prop.Value!);
            }
        }
        return result;
    }

    private Value ConvertJsonNodeToValue(JsonNode node)
    {
        return node switch
        {
            JsonValue val when val.TryGetValue(out bool b) => Value.ForBool(b),
            JsonValue val when val.TryGetValue(out string? s) => Value.ForString(s),
            JsonValue val when val.TryGetValue(out double d) => Value.ForNumber(d),
            JsonValue val when val.TryGetValue(out int i) => Value.ForNumber(i),
            JsonValue val when val.TryGetValue(out long l) => Value.ForNumber(l),
            JsonArray array => Value.ForList(array.Select(ConvertJsonNodeToValue!).ToArray()),
            JsonObject obj => Value.ForStruct(ConvertJsonNodeToStruct(obj)),
            _ => Value.ForNull()
        };
    }

    private static LogSeverity ConvertSeverity(LogLevel level) => level switch
    {
        LogLevel.Trace => LogSeverity.Debug,
        LogLevel.Debug => LogSeverity.Debug,
        LogLevel.Information => LogSeverity.Info,
        LogLevel.Warning => LogSeverity.Warning,
        LogLevel.Error => LogSeverity.Error,
        LogLevel.Critical => LogSeverity.Critical,
        _ => LogSeverity.Default,
    };

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    // public void Log<TState>(LogLevel logLevel, string payload, object data)
    // {
        // Console.WriteLine("LOgged");

        // if (!IsEnabled(logLevel)) return;

        // LogName logName = new LogName(_projectId, _category);
        // string jsonString = JsonSerializer.Serialize(data);
        // Struct jsonStruct = JsonParser.Default.Parse<Struct>(jsonString);

        // LogEntry entry = new LogEntry
        // {
        //     LogName = logName.ToString(),
        //     Severity = ConvertSeverity(logLevel),
        //     Timestamp = Timestamp.FromDateTime(DateTime.UtcNow),
        //     TextPayload = payload,
        //     JsonPayload = jsonStruct,
        //     Resource = new MonitoredResource { Type = "global" },
        // };

        // _client.WriteLogEntries(logName: logName, resource: entry.Resource, labels: null, entries: new[] { entry });
    // }
}

public class GoogleApiLoggerProvider : ILoggerProvider
{
    private readonly LoggingServiceV2Client _client;
    private readonly string _projectId;

    public GoogleApiLoggerProvider(LoggingServiceV2Client client, string projectId)
    {
        _client = client;
        _projectId = projectId;
    }

    public ILogger CreateLogger(string categoryName)
    {
        // Console.WriteLine("Category NAME IS {0}", categoryName);
        return new GoogleApiLogger(categoryName, _client, _projectId);
    }

    public void Dispose() { }
}

