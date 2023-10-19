using System;
using Unity.Mathematics;
using UnityEngine;

namespace Terrain.Graph
{
    public class PositionNode : TerrainNode
    {
        public override void Initialize(TerrainGraphView graphView, Vector2 position)
        {
            title = "Position";

            Vector2 graphPosition = graphView.viewTransform.matrix.inverse.MultiplyPoint(position);
            base.SetPosition(new Rect(graphPosition.x, graphPosition.y, 100, 150));

            GUID = UnityEditor.GUID.Generate();

            graphView.AddElement(this);
        }

        public override void Draw()
        {
            /* OUTPUT CONTAINER */
            TerrainGraphElementUtility.AddPort(this, "Out(3)", typeof(float3), true);

            RefreshExpandedState();
        }

        public override SaveData GetSaveData()
        {
            return new PositionNodeSaveData(this);
        }

        public class PositionNodeSaveData : SaveData
        {
            public PositionNodeSaveData(PositionNode positionNode) : base(positionNode) { }

            public override void Load(TerrainGraphView graphView)
            {
                PositionNode positionNode = (PositionNode)Activator.CreateInstance(typeof(PositionNode));
                positionNode.Initialize(graphView, new(positionX, positionY));
            }
        }
    }
}
