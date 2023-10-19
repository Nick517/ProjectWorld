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
            Draw();
        }

        public override void Draw()
        {
            /* INPUT CONTAINER */
            TerrainGraphElementUtility.AddPort(this, "In(1)", typeof(float));

            RefreshExpandedState();
        }

        #region Save System
        public override SaveData GetSaveData()
        {
            return new SampleNodeSaveData(this);
        }

        public class SampleNodeSaveData : SaveData
        {
            public SampleNodeSaveData() : base() { }

            public SampleNodeSaveData(SampleNode sampleNode) : base(sampleNode) { }
        }
        #endregion
    }
}
