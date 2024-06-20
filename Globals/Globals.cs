using System.Text.Json;

namespace Global;

public static class Helper
{
    public static byte[] ConvertToBytes(object obj)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(obj);
        return bytes;
    }
}