using System.IO;
using Newtonsoft.Json;

namespace Editor.Terrain.Generation
{
    public static class SaveManager
    {
        public static void Save(TgGraph graph)
        {
            var dto = graph.ToDto();

            var json = JsonConvert.SerializeObject(dto, SerializerSettings);

            File.WriteAllText(graph.path, json);
        }

        public static void Load(TgGraph graph)
        {
            var json = File.ReadAllText(graph.path);

            var tgGraphViewDto = JsonConvert.DeserializeObject<TgGraph.Dto>(json, SerializerSettings);

            tgGraphViewDto.Deserialize(graph);
        }

        private static JsonSerializerSettings SerializerSettings =>
            new()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            };
    }
}