using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using static TerrainGenerationGraph.NodeDefinitions;
using static TerrainGenerationGraph.NodeOperations;

namespace TerrainGenerationGraph
{
    [CreateAssetMenu(fileName = "New Terrain Generation Graph", menuName = "Terrain Generation Graph", order = 1)]
    public class TgGraph : ScriptableObject
    {
        public List<Node> nodes = new() { new Node { type = NodeDefinitions.Type.Sample } };
        public List<Edge> edges = new();

        public void BuildGraph()
        {
            var idToInputPort = new Dictionary<string, Node.InputPort>();
            var idToOutputPort = new Dictionary<string, Node.OutputPort>();

            foreach (var node in nodes)
            {
                for (var i = 0; i < node.Definition.InputPorts.Count; i++)
                {
                    var inputPort = node.inputPorts[i];
                    idToInputPort[inputPort.id] = inputPort;
                }

                for (var i = 0; i < node.Definition.OutputPorts.Count; i++)
                {
                    var outputPort = node.outputPorts[i];
                    idToOutputPort[outputPort.id] = outputPort;
                    outputPort.ParentNode = node;
                    outputPort.Operation = node.Definition.OutputPorts[i].Operation;
                }
            }

            foreach (var edge in edges)
            {
                var outputPort = idToOutputPort[edge.outputPortId];
                if (outputPort.CacheIndex != -3) outputPort.CacheIndex--;
                idToInputPort[edge.inputPortId].ConnectedPort = outputPort;
            }
        }

        [Serializable]
        public class Node
        {
            public NodeDefinitions.Type type;
            public Vector2 position;
            public List<InputPort> inputPorts = new();
            public List<OutputPort> outputPorts = new();

            [Serializable]
            public class InputPort
            {
                public string id;
                public float4 constVal;

                [NonSerialized] public OutputPort ConnectedPort;
            }

            [Serializable]
            public class OutputPort
            {
                public string id;

                [NonSerialized] public Node ParentNode;
                [NonSerialized] public Operation Operation;
                [NonSerialized] public int CacheIndex = -1;
            }

            public Definition Definition => Definitions[type];
        }

        [Serializable]
        public class Edge
        {
            public string inputPortId;
            public string outputPortId;
        }
    }
}