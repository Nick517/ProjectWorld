using UnityEngine;

namespace Terrain.Graph
{
    public class AddNode : TerrainNode
    {
        public override void Initialize(TerrainGraphView graphView, Vector2 position)
        {
            title = "Add";

            Vector2 graphPosition = graphView.viewTransform.matrix.inverse.MultiplyPoint(position);
            base.SetPosition(new Rect(graphPosition.x, graphPosition.y, 100, 150));

            graphView.AddElement(this);
        }
    }
}
