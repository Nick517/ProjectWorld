using Newtonsoft.Json;
using TerrainGenerationGraph.Scripts.Nodes;
using UnityEngine;

namespace TerrainGenerationGraph.Scripts
{
    [CreateAssetMenu(fileName = "New TGGraph", menuName = "TGGraph", order = 1)]
    public class TgGraph : ScriptableObject
    {
        [HideInInspector] public string serializedTreeData;
        [HideInInspector] public string serializedGraphData;

        private SampleTgtNode _rootTgtNode;

        public void Initialize()
        {
            _rootTgtNode = JsonConvert.DeserializeObject<SampleTgtNode>(serializedTreeData, JsonSettings.Formatted);
        }

        public float GetSample()
        {
            return _rootTgtNode.Traverse();
        }
    }
}