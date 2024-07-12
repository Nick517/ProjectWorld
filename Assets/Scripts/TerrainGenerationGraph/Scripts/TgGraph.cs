using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Serializable;
using UnityEngine;
using static JsonSettings;
using static NodeOperations.Operation;

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

        [Serializable]
        public class TgTreeDto
        {
            public List<TgtNodeDto> nodes = new();
            public List<SerializableFloat4> values = new();

            public TgTreeDto()
            {
            }

            public TgTreeDto(TgtNodeDto rootNode)
            {
                AddNode(rootNode);
            }

            public void AddNode(TgtNodeDto node)
            {
                if (nodes.Contains(node)) return;

                nodes.Add(node);

                if (node.operation == Value)
                {
                    AddValue(node);

                    return;
                }

                List<int> nextIndexList = new();

                foreach (var nextNode in node.nextNodes)
                {
                    AddNode(nextNode);
                    nextIndexList.Add(nodes.IndexOf(nextNode));
                }

                if (nextIndexList.Count > 0) node.nextIndex.x = nextIndexList.ElementAt(0);
                if (nextIndexList.Count > 1) node.nextIndex.y = nextIndexList.ElementAt(1);
                if (nextIndexList.Count > 2) node.nextIndex.z = nextIndexList.ElementAt(2);
                if (nextIndexList.Count > 3) node.nextIndex.w = nextIndexList.ElementAt(3);
            }

            public void AddValue(TgtNodeDto node)
            {
                values.Add(new SerializableFloat4(node.value));
                node.nextIndex.x = values.Count - 1;
            }
        }
    }
}