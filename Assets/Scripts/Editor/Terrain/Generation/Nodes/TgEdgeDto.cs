using System;
using UnityEditor.Experimental.GraphView;

namespace Editor.Terrain.Generation.Nodes
{
    [Serializable]
    public class TgEdgeDto
    {
        public string inputPortId;
        public string outputPortId;

        public TgEdgeDto()
        {
        }

        public TgEdgeDto(TgGraphView graph, Edge edge)
        {
            inputPortId = graph.GetTgPort(edge.input).id;
            outputPortId = graph.GetTgPort(edge.output).id;
        }

        public void Deserialize(TgGraphView graph)
        {
            var inputPort = graph.GetTgPort(inputPortId);
            var outputPort = graph.GetTgPort(outputPortId);
            var edge = inputPort.port.ConnectTo(outputPort.port);
            graph.AddElement(edge);
        }
    }
}