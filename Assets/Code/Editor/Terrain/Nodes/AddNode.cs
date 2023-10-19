using System;
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

            GUID = UnityEditor.GUID.Generate();

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

        public override SaveData GetSaveData()
        {
            return new AddNodeSaveData(this);
        }

        public class AddNodeSaveData : SaveData
        {
            public AddNodeSaveData(AddNode addNode) : base(addNode) { }

            public override void Load(TerrainGraphView graphView)
            {
                AddNode addNode = (AddNode)Activator.CreateInstance(typeof(AddNode));
                addNode.Initialize(graphView, new(positionX, positionY));
            }
        }
    }
}
