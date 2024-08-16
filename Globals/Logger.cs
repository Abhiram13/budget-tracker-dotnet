using System.Text.Json;

namespace BudgetTracker.Application;

public static class Logger
{
    public static void Log<T>(T arg)
    {
        Console.WriteLine(JsonSerializer.Serialize(arg));
    }
}