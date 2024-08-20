using Newtonsoft.Json;
using UnityEngine;
using static Utility.Serialization.JsonSettings;

namespace TerrainGenerationGraph
{
    [CreateAssetMenu(fileName = "New Terrain Generation Graph", menuName = "Terrain Generation Graph", order = 1)]
    public class TgGraph : ScriptableObject
    {
        [HideInInspector] public string serializedTreeData;
        [HideInInspector] public string serializedGraphData;

        public TgTreeDto DeserializeTree()
        {
            return JsonConvert.DeserializeObject<TgTreeDto>(serializedTreeData, Formatted);
        }
    }
}