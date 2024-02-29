using System;
using UnityEditor.Experimental.GraphView;

namespace Editor.Terrain.Generation.Nodes
{
    public class TgPort
    {
        public string id;
        public readonly Port port;

        public TgPort(TgGraphView graph, TgNode parentNode, Orientation orientation, Direction direction,
            Port.Capacity capacity,
            Type type)
        {
            id = Guid.NewGuid().ToString();
            port = parentNode.InstantiatePort(orientation, direction, capacity, type);
            graph.tgPorts.Add(this);
        }
    }
}