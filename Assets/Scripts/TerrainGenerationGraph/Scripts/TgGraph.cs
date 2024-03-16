using Newtonsoft.Json;
using TerrainGenerationGraph.Scripts.Nodes;
using UnityEngine;

namespace TerrainGenerationGraph.Scripts
{
    [CreateAssetMenu(fileName = "New Terrain Generation Graph", menuName = "Terrain Generation Graph", order = 1)]
    public class TgGraph : ScriptableObject
    {
        [HideInInspector] public string serializedTreeData;
        [HideInInspector] public string serializedGraphData;

        private SampleTgtNode _rootTgtNode;

        public void Initialize()
        {
            _rootTgtNode = JsonConvert.DeserializeObject<SampleTgtNode>(serializedTreeData, JsonSettings.Formatted);
        }

        public Vector4 GetSample()
        {
            return _rootTgtNode.Traverse();
        }
    }
}