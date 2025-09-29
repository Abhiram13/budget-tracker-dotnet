using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BudgetTracker.Shared.Utility;

public class UtcJsonConvertor : JsonConverter<DateTime>
{
    private readonly string _timeZoneId = "India Standard Time";
    private readonly string _format = "yyyy-MM-dd";

    public UtcJsonConvertor() { }

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTime.Parse(reader.GetString()!);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(value, TimeZoneInfo.FindSystemTimeZoneById(_timeZoneId));
        writer.WriteStringValue(localTime.ToString(_format));
    }
}