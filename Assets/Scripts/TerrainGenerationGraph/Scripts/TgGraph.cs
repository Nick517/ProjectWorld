using Newtonsoft.Json;
using UnityEngine;
using static JsonSettings;

namespace TerrainGenerationGraph.Scripts
{
    [CreateAssetMenu(fileName = "New Terrain Generation Graph", menuName = "Terrain Generation Graph", order = 1)]
    public class TgGraph : ScriptableObject
    {
        public string serializedTreeData;
        public string serializedGraphData;

        public TgTreeDto DeserializeTree()
        {
            return JsonConvert.DeserializeObject<TgTreeDto>(serializedTreeData, Formatted);
        }
    }
}