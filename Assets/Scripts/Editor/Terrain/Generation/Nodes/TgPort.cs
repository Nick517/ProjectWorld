using UnityEditor.Experimental.GraphView;

namespace Editor.Terrain.Generation.Nodes
{
    public class TgPort
    {
        public string id;
        public readonly Port port;

        public TgPort(Port port)
        {
            this.port = port;
            id = GraphUtil.NewID;
        }
    }
}