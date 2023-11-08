using System;
using UnityEditor.Experimental.GraphView;

namespace Terrain.Graph
{
    public class NodePort : Port
    {
        public string GUID;

        public NodePort(Orientation portOrientation, Direction direction, Capacity capacity, Type type) : base(portOrientation, direction, capacity, type)
        {
            GUID = UnityEditor.GUID.Generate().ToString();
        }

        public string GetEdgeGUID()
        {
            NodePort port = (NodePort)connections.GetEnumerator();
            return port.GUID;
        }
    }
}
