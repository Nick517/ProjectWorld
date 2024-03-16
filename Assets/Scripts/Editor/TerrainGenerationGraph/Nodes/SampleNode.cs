using System;
using Editor.TerrainGenerationGraph.Nodes.NodeComponents;
using TerrainGenerationGraph.Scripts.Nodes;
using UnityEditor.Experimental.GraphView;

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
            title = "Sample";

            _inputPort = AddInputPort();

            capabilities &= ~Capabilities.Deletable;
        }

        #endregion

        #region Terrain Generation Tree

        public override TgtNode ToTgtNode()
        {
            return new SampleTgtNode
            {
                nextNode = _inputPort.NextTgtNode
            };
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

            public override TggNode Deserialize(TerrainGenerationGraphView graphView)
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