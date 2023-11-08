using System;
using UnityEngine;

namespace Terrain.Graph
{
    public class AddNode : TerrainNode
    {
        public void Initialize(TerrainGraphView graphView, Vector2 position)
        {
            base.Initialize(graphView, position);
        }

        public override void Draw()
        {
            title = "Add";
            SetDimensions(150, 100);

            /* INPUT CONTAINER */
            AddInputPort("A(1)", typeof(float));
            AddInputPort("B(1)", typeof(float));

            /* OUTPUT CONTAINER */
            AddOutputPort("Out(1)", typeof(float));

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

            public override void LoadConnections(TerrainNode terrainNode, TerrainGraphView graphView)
            {
                throw new NotImplementedException();
            }
        }
    }
}
