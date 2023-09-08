using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Terrain.Graph
{
    public abstract class TerrainNode : Node
    {
        public TerrainNode()
        {
            // Constructor logic, if needed
        }

        public abstract void Instantiate(TerrainGraphView graphView, Vector2 position);
    }
}
