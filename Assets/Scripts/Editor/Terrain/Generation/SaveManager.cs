using System.IO;
using Newtonsoft.Json;

namespace Editor.Terrain.Generation
{
    public static class SaveManager
    {
        public static void Save(TgGraphView graph)
        {
            var dto = graph.ToDto();

            var json = graph.ToJson();

            File.WriteAllText(graph.path, json);
        }

        public static void Load(TgGraphView graph)
        {
            var json = File.ReadAllText(graph.path);

            var tgGraphViewDto = JsonConvert.DeserializeObject<TgGraphView.Dto>(json, SerializerSettings);

            tgGraphViewDto.Deserialize(graph);
        }

        public static JsonSerializerSettings SerializerSettings =>
            new()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            };
    }
}