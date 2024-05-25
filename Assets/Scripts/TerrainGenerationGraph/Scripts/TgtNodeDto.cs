using System;
using System.Linq;
using Newtonsoft.Json;
using Serializable;
using Unity.Mathematics;
using static NodeOperations;

namespace TerrainGenerationGraph.Scripts
{
    [Serializable]
    public class TgtNodeDto
    {
        public NodeType nodeType;
        public SerializableInt4 nextIndex;
        public bool cached;

        [JsonIgnore] public TgtNodeDto[] nextNodes;
        [JsonIgnore] public float4 value;

        public TgtNodeDto()
        {
        }

        public TgtNodeDto(NodeType nodeType, TgtNodeDto[] nextNodes, float4 value = default)
        {
            this.nodeType = nodeType;
            nextIndex = new SerializableInt4(-1);
            this.nextNodes = nextNodes;
            this.value = value;
        }

        public void Simplify()
        {
            foreach (var node in nextNodes) node.Simplify();

            value = nodeType switch
            {
                NodeType.Value =>
                    value,

                _ =>
                    GetSample(
                        nodeType,
                        nextNodes.Length > 0 ? nextNodes[0].value : default,
                        nextNodes.Length > 1 ? nextNodes[1].value : default,
                        nextNodes.Length > 2 ? nextNodes[2].value : default,
                        nextNodes.Length > 3 ? nextNodes[3].value : default)
            };

            if (nextNodes.Any() && nextNodes.All(node => node.nodeType == NodeType.Value))
            {
                nodeType = NodeType.Value;
                nextNodes = new TgtNodeDto[] { };
            }
        }
    }
}