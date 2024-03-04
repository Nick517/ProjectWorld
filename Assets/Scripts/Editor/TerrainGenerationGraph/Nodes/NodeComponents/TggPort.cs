using TerrainGenerationGraph.Scripts.Nodes;
using UnityEditor.Experimental.GraphView;

namespace Editor.TerrainGenerationGraph.Nodes.NodeComponents
{
    public class TggPort
    {
        private readonly TerrainGenGraphView _graphView;

        public string id;
        public readonly Port port;

        public TggPort(TerrainGenGraphView graphView, Port port)
        {
            _graphView = graphView;
            this.port = port;
            id = GraphUtil.NewID;
        }

        private TggPort GetConnectedTggPort()
        {
            var tggPorts = _graphView.TggPorts;

            foreach (var tggEdge in _graphView.GetAllTggEdgeDto())
            {
                if (tggEdge.inputPortId == id)
                    foreach (var tggPort in tggPorts)
                        if (tggPort.id == tggEdge.outputPortId)
                            return tggPort;

                if (tggEdge.outputPortId == id)
                    foreach (var tggPort in tggPorts)
                        if (tggPort.id == tggEdge.inputPortId)
                            return tggPort;
            }

            return null;
        }

        private TggNode GetParentTggNode()
        {
            foreach (var tggNode in _graphView.TggNodes)
                if (tggNode.TggPorts.Contains(this))
                    return tggNode;

            return null;
        }

        private TggNode GetConnectedTggNode()
        {
            return GetConnectedTggPort()?.GetParentTggNode();
        }

        public TgtNode GetConnectedTgtNode()
        {
            return GetConnectedTggNode()?.ToTgtNode();
        }
    }
}