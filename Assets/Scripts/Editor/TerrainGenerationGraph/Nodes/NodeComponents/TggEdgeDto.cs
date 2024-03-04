using System;
using UnityEditor.Experimental.GraphView;

namespace Editor.TerrainGenerationGraph.Nodes.NodeComponents
{
    [Serializable]
    public class TggEdgeDto
    {
        #region Fields

        public string inputPortId;
        public string outputPortId;

        #endregion

        #region Constructors

        public TggEdgeDto()
        {
        }

        public TggEdgeDto(TerrainGenGraphView graphView, Edge edge)
        {
            inputPortId = graphView.GetTggPort(edge.input).id;
            outputPortId = graphView.GetTggPort(edge.output).id;
        }

        #endregion

        #region Serialization

        public void Deserialize(TerrainGenGraphView graphView)
        {
            var inputPort = graphView.GetTggPort(inputPortId);
            var outputPort = graphView.GetTggPort(outputPortId);
            var edge = inputPort.port.ConnectTo(outputPort.port);
            graphView.AddElement(edge);
        }

        #endregion
    }
}