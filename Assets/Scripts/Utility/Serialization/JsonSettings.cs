using Newtonsoft.Json;

namespace Utility.Serialization
{
    public static class JsonSettings
    {
        public static JsonSerializerSettings Formatted => new()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented
        };
    }
}