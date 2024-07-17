using System;
using System.Linq;
using Newtonsoft.Json;
using Serializable;
using Unity.Mathematics;
using static NodeOperations;

namespace TerrainGenerationGraph.Scripts
{
    [Serializable]
    public class TreeNodeDto
    {
        public Operation operation;
        public SerializableInt4 nextIndex = new int4(-1);
        public bool cached;

        [JsonIgnore] public TreeNodeDto[] nextNodes;
        [JsonIgnore] public float4 value;

        public TreeNodeDto()
        {
        }

        public TreeNodeDto(Operation operation, TreeNodeDto[] nextNodes, float4 value = default)
        {
            this.operation = operation;
            this.nextNodes = nextNodes;
            this.value = value;
        }

        public void Simplify()
        {
            foreach (var node in nextNodes) node.Simplify();

            value = operation switch
            {
                Operation.Value => value,

                _ => GetOutput(
                    operation,
                    nextNodes.Length > 0 ? nextNodes[0].value : default,
                    nextNodes.Length > 1 ? nextNodes[1].value : default,
                    nextNodes.Length > 2 ? nextNodes[2].value : default,
                    nextNodes.Length > 3 ? nextNodes[3].value : default)
            };

            if (nextNodes.Any() && nextNodes.All(node => node.operation == Operation.Value))
            {
                operation = Operation.Value;
                nextNodes = new TreeNodeDto[] { };
            }
        }
    }
}