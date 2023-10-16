using UnityEngine;

namespace Terrain.Graph
{
    public class SampleNode : TerrainNode
    {
        public override void Initialize(TerrainGraphView graphView, Vector2 position)
        {
            title = "Sample";

            Vector2 graphPosition = graphView.viewTransform.matrix.inverse.MultiplyPoint(position);
            base.SetPosition(new Rect(graphPosition.x, graphPosition.y, 100, 150));

            GUID = UnityEditor.GUID.Generate();

            graphView.AddElement(this);
        }

        public override void Draw()
        {
            /* INPUT CONTAINER */
            TerrainGraphElementUtility.AddPort(this, "In(1)", typeof(float));

            RefreshExpandedState();
        }

        public override TerrainNodeSaveData GetSaveData()
        {
            return new SampleNodeSaveData(this);
        }

        public class SampleNodeSaveData : TerrainNodeSaveData
        {
            public SampleNodeSaveData(SampleNode sampleNode) : base(sampleNode) { }
        }
    }
}
