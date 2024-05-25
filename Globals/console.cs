using System.Text.Json;

namespace Global;

public static class Logger
{
    public static void Log<T>(T arg)
    {
        Console.WriteLine(JsonSerializer.Serialize(arg));
    }
}