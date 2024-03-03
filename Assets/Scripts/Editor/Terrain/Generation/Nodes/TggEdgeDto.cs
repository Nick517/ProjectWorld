using System;
using UnityEditor.Experimental.GraphView;

namespace Editor.Terrain.Generation.Nodes
{
    [Serializable]
    public class TggEdgeDto
    {
        public string inputPortId;
        public string outputPortId;

        public TggEdgeDto()
        {
        }

        public TggEdgeDto(TerrainGenGraphView graph, Edge edge)
        {
            inputPortId = graph.GetTggPort(edge.input).id;
            outputPortId = graph.GetTggPort(edge.output).id;
        }

        public void Deserialize(TerrainGenGraphView graphView)
        {
            var inputPort = graphView.GetTggPort(inputPortId);
            var outputPort = graphView.GetTggPort(outputPortId);
            var edge = inputPort.port.ConnectTo(outputPort.port);
            graphView.AddElement(edge);
        }
    }
}