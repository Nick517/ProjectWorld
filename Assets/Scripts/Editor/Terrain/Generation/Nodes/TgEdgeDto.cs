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

        public TgEdgeDto(TgGraphView graphView, Edge edge)
        {
            inputPortId = graphView.GetTgPort(edge.input).id;
            outputPortId = graphView.GetTgPort(edge.output).id;
        }

        public void Deserialize(TgGraphView graphView)
        {
            var inputPort = graphView.GetTgPort(inputPortId);
            var outputPort = graphView.GetTgPort(outputPortId);
            var edge = inputPort.port.ConnectTo(outputPort.port);
            graphView.AddElement(edge);
        }
    }
}