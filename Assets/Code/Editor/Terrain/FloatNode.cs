using UnityEngine;

namespace Terrain.Graph
{
    public class FloatNode : TerrainNode
    {
        public override void Initialize(TerrainGraphView graphView, Vector2 position)
        {
            title = "Float";

            Vector2 graphPosition = graphView.viewTransform.matrix.inverse.MultiplyPoint(position);
            base.SetPosition(new Rect(graphPosition.x, graphPosition.y, 100, 150));

            graphView.AddElement(this);
        }
    }
}
