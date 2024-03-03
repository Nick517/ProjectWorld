using System;
using System.Collections.Generic;

namespace Editor.Terrain.Generation.Nodes
{
    public class SampleNode : TggNode
    {
        #region Fields

        private TggPort _inputPort;

        public override List<TggPort> TggPorts => new() { _inputPort };

        #endregion

        #region Methods

        protected override void SetUp()
        {
            title = "Sample";

            _inputPort = AddInputPort("In(1)", typeof(float));
        }

        #endregion

        #region Terrain Genereration Tree

        public override TgtNode ToTgtNode()
        {
            return new SampleTgtNode(this);
        }

        private class SampleTgtNode : TgtNode
        {
            private readonly TgtNode _inputNode;

            public SampleTgtNode(SampleNode sampleNode)
            {
                _inputNode = sampleNode._inputPort.GetConnectedTgtNode();
            }

            public override float Traverse()
            {
                return _inputNode.Traverse();
            }
        }

        #endregion

        #region Serialization

        public override Dto ToDto()
        {
            return new SampleNodeDto(this);
        }

        [Serializable]
        public class SampleNodeDto : Dto
        {
            public string inputPortId;

            public SampleNodeDto()
            {
            }

            public SampleNodeDto(SampleNode sampleNode) : base(sampleNode)
            {
                inputPortId = sampleNode._inputPort.id;
            }

            public override TggNode Deserialize(TerrainGenGraphView graphView)
            {
                var sampleNode = (SampleNode)Create(graphView, typeof(SampleNode));
                sampleNode._inputPort.id = inputPortId;
                sampleNode.id = id;
                sampleNode.SetPosition(position.Deserialize());

                return sampleNode;
            }
        }

        #endregion
    }
}