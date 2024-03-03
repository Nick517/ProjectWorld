using UnityEditor.Experimental.GraphView;

namespace Editor.Terrain.Generation.Nodes
{
    public class TggPort
    {
        private readonly TerrainGenGraphView _graph;

        public string id;
        public readonly Port port;

        public TggPort(TerrainGenGraphView graph, Port port)
        {
            _graph = graph;
            this.port = port;
            id = GraphUtil.NewID;
        }

        private TggPort GetConnectedTggPort()
        {
            var tggPorts = _graph.TggPorts;

            foreach (var tggEdge in _graph.GetAllTggEdgeDto())
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
            foreach (var tggNode in _graph.TggNodes)
                if (tggNode.TggPorts.Contains(this))
                    return tggNode;

            return null;
        }

        private TggNode GetConnectedTggNode()
        {
            return GetConnectedTggPort().GetParentTggNode();
        }

        public TggNode.TgtNode GetConnectedTgtNode()
        {
            return GetConnectedTggNode().ToTgtNode();
        }
    }
}