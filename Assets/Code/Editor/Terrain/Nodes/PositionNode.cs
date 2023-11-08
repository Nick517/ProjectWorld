using System;
using Unity.Mathematics;
using UnityEngine;

namespace Terrain.Graph
{
    public class PositionNode : TerrainNode
    {
        public void Initialize(TerrainGraphView graphView, Vector2 position)
        {
            base.Initialize(graphView, position);
        }

        public override void Draw()
        {
            title = "Position";
            SetDimensions(150, 100);

            /* OUTPUT CONTAINER */
            AddOutputPort("Out(3)", typeof(float3));

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

            public override void LoadConnections(TerrainNode terrainNode, TerrainGraphView graphView)
            {
                throw new NotImplementedException();
            }
        }
    }
}
