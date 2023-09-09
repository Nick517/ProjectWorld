using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Terrain.Graph
{
    public abstract class TerrainNode : Node
    {
        public abstract void Initialize(TerrainGraphView graphView, Vector2 position);

        public abstract void Draw();
    }
}
