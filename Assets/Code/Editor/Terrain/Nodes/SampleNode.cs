using System;

namespace Terrain.Graph
{
    public class SampleNode : TerrainNode
    {
        public NodePort inputPort;

        public override void Draw()
        {
            title = "Sample";
            SetDimensions(150, 100);

            /* INPUT CONTAINER */
            inputPort = AddInputPort("In(1)", typeof(float));

            RefreshExpandedState();
            RefreshPorts();
        }

        #region Save System
        public override SaveData GetSaveData()
        {
            return new SampleNodeSaveData(this);
        }

        public class SampleNodeSaveData : SaveData
        {
            public string inputPortEdgeGUID;

            public SampleNodeSaveData() : base() { }

            public SampleNodeSaveData(SampleNode sampleNode) : base(sampleNode)
            {
                inputPortEdgeGUID = sampleNode.inputPort.GetEdgeGUID().ToString();
            }

            public override void Load(TerrainGraphView graphView)
            {
                SampleNode sampleNode = (SampleNode)Activator.CreateInstance(typeof(SampleNode));
                sampleNode.Initialize(graphView, new(positionX, positionY), GUID);
            }

            public override void LoadConnections(TerrainNode terrainNode, TerrainGraphView graphView)
            {
                SampleNode sampleNode = (SampleNode)terrainNode;
                sampleNode.inputPort = graphView.GetPortFromGUID(inputPortEdgeGUID);
            }
        }
        #endregion
    }
}
