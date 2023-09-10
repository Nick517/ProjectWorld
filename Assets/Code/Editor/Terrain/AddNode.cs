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

        public override void Draw()
        {
            /* INPUT CONTAINER */
            TerrainGraphElementUtility.AddPort(this, "A(1)", typeof(float));
            TerrainGraphElementUtility.AddPort(this, "B(1)", typeof(float));

            /* OUTPUT CONTAINER */
            TerrainGraphElementUtility.AddPort(this, "Out(1)", typeof(float), true);

            RefreshExpandedState();
        }
    }
}
