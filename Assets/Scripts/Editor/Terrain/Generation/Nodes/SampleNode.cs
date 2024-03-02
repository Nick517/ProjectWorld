using System;
using System.Collections.Generic;

namespace Editor.Terrain.Generation.Nodes
{
    public class SampleNode : TgNode
    {
        
        #region Fields
        
        private TgPort _inputPort;

        public override List<TgPort> TgPorts => new() { _inputPort };
        
        #endregion
        
        #region Methods
        
        protected override void SetUp()
        {
            title = "Sample";

            _inputPort = AddInputPort("In(1)", typeof(float));
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

            public override TgNode Deserialize(TgGraph graph)
            {
                var sampleNode = (SampleNode)Create(graph, typeof(SampleNode));
                sampleNode._inputPort.id = inputPortId;
                sampleNode.id = id;
                sampleNode.SetPosition(position.Deserialize());

                return sampleNode;
            }
        }
        
        #endregion
    }
}