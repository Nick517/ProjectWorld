using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Serializable;
using UnityEngine;

namespace TerrainGenerationGraph.Scripts
{
    [CreateAssetMenu(fileName = "New Terrain Generation Graph", menuName = "Terrain Generation Graph", order = 1)]
    public class TgGraph : ScriptableObject
    {
        public string serializedTreeData;
        public string serializedGraphData;

        public TgTreeDto DeserializeTree()
        {
            return JsonConvert.DeserializeObject<TgTreeDto>(serializedTreeData, JsonSettings.Formatted);
        }

        [Serializable]
        public class TgTreeDto
        {
            public List<TgtNodeDto> nodes = new();
            public List<SerializableFloat4> values = new();
        }
    }
}