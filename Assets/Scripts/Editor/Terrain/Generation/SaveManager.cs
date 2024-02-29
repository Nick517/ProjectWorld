using System.IO;
using Newtonsoft.Json;

namespace Editor.Terrain.Generation
{
    public static class SaveManager
    {
        public static void Save(TgGraphView graph, string path)
        {
            var dto = graph.ToDto();

            var json = JsonConvert.SerializeObject(dto, SerializerSettings);
            
            File.WriteAllText(path, json);
        }

        public static void Load(TgGraphView graph, string path)
        {
            var json = File.ReadAllText(path);

            JsonConvert.DeserializeObject<TgGraphView.Dto>(json, SerializerSettings);
        }

        private static JsonSerializerSettings SerializerSettings =>
            new()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            };
    }
}