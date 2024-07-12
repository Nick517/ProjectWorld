using System;
using System.Linq;
using Newtonsoft.Json;
using Serializable;
using Unity.Mathematics;
using UnityEngine.Serialization;
using static NodeOperations;

namespace TerrainGenerationGraph.Scripts
{
    [Serializable]
    public class TgtNodeDto
    {
        [FormerlySerializedAs("nodeType")] public Operation operation;
        public SerializableInt4 nextIndex;
        public bool cached;

        [JsonIgnore] public TgtNodeDto[] nextNodes;
        [JsonIgnore] public float4 value;

        public TgtNodeDto()
        {
        }

        public TgtNodeDto(Operation operation, TgtNodeDto[] nextNodes, float4 value = default)
        {
            this.operation = operation;
            nextIndex = new SerializableInt4(-1);
            this.nextNodes = nextNodes;
            this.value = value;
        }

        public void Simplify()
        {
            foreach (var node in nextNodes) node.Simplify();

            value = operation switch
            {
                Operation.Value =>
                    value,

                _ =>
                    GetSample(
                        operation,
                        nextNodes.Length > 0 ? nextNodes[0].value : default,
                        nextNodes.Length > 1 ? nextNodes[1].value : default,
                        nextNodes.Length > 2 ? nextNodes[2].value : default,
                        nextNodes.Length > 3 ? nextNodes[3].value : default)
            };

            if (nextNodes.Any() && nextNodes.All(node => node.operation == Operation.Value))
            {
                operation = Operation.Value;
                nextNodes = new TgtNodeDto[] { };
            }
        }
    }
}