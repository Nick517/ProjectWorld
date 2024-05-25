using System;
using Editor.TerrainGenerationGraph.Nodes.NodeComponents;
using UnityEditor.Experimental.GraphView;
using static NodeOperations;

namespace Editor.TerrainGenerationGraph.Nodes
{
    public class SampleNode : TggNode, ITggNodeSerializable
    {
        #region Fields

        private InputPort _inputPort;

        #endregion

        #region Methods

        protected override void SetUp()
        {
            NodeType = NodeType.Skip;

            title = "Sample";

            _inputPort = AddInputPort();

            capabilities &= ~Capabilities.Deletable;
        }

        #endregion

        #region Serialization

        public Dto ToDto()
        {
            return new SampleNodeDto(this);
        }

        [Serializable]
        public class SampleNodeDto : Dto
        {
            public InputPort.Dto inputPortDto;

            public SampleNodeDto()
            {
            }

            public SampleNodeDto(SampleNode sampleNode) : base(sampleNode)
            {
                inputPortDto = sampleNode._inputPort.ToDto();
            }

            public override TggNode Deserialize(TerrainGenGraphView graphView)
            {
                var sampleNode = (SampleNode)Create(graphView, typeof(SampleNode));

                DeserializeTo(sampleNode);
                inputPortDto.DeserializeTo(sampleNode._inputPort);

                return sampleNode;
            }
        }

        #endregion
    }
}