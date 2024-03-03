using System.IO;
using Newtonsoft.Json;

namespace Editor.Terrain.Generation
{
    public static class SaveManager
    {
        public static void Save(TerrainGenGraphView graph)
        {
            File.WriteAllText(graph.path, graph.ToJson());
        }

        public static void Load(TerrainGenGraphView graph)
        {
            var json = File.ReadAllText(graph.path);

            var tgGraphViewDto = JsonConvert.DeserializeObject<TerrainGenGraphView.Dto>(json, SerializerSettings);

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