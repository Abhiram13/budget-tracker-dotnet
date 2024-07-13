using System.Text.Json;

namespace BudgetTracker.Application;

public static class Logger
{
    public static void Log<T>(T arg)
    {
        Console.WriteLine(JsonSerializer.Serialize(arg));
    }
}

public static class ResponseBytes
{
    public static byte[] Convert(object obj)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(obj);
        return bytes;
    }
}