using UnityEngine;

namespace Terrain.Graph
{
    public class FloatNode : TerrainNode
    {
        public FloatNode()
        {
            // Constructor logic, if needed
        }

        public override void Instantiate(TerrainGraphView graphView, Vector2 position)
        {
            title = "Float";

            Vector2 graphPosition = graphView.viewTransform.matrix.inverse.MultiplyPoint(position);
            base.SetPosition(new Rect(graphPosition.x, graphPosition.y, 100, 150));

            graphView.AddElement(this);
        }
    }
}
