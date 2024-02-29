using System;
using UnityEditor.Experimental.GraphView;

namespace Editor.Terrain.Generation.Nodes
{
    public class TgEdge
    {
        private readonly TgGraphView _graph;

        public readonly Edge edge;

        private TgPort InputPort => _graph.GetTgPort(edge.input);
        private TgPort OutputPort => _graph.GetTgPort(edge.output);

        public TgEdge(TgGraphView graph, Edge edge)
        {
            _graph = graph;
            this.edge = edge;
            graph.tgEdges.Add(this);
        }

        public Dto ToDto()
        {
            return new Dto(this);
        }

        [Serializable]
        public class Dto
        {
            public string inputPortId;
            public string outputPortId;

            public Dto(TgEdge tgEdge)
            {
                inputPortId = tgEdge.InputPort.id;
                outputPortId = tgEdge.OutputPort.id;
            }

            public TgEdge Deserialize(TgGraphView graph)
            {
                var inputPort = graph.GetTgPort(inputPortId);
                var outputPort = graph.GetTgPort(outputPortId);

                var edge = inputPort.port.ConnectTo(outputPort.port);

                return new TgEdge(graph, edge);
            }
        }
    }
}