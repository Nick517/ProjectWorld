using Newtonsoft.Json;

public static class JsonSettings
{
    public static JsonSerializerSettings Formatted => new()
    {
        TypeNameHandling = TypeNameHandling.Auto,
        Formatting = Formatting.Indented
    };
}