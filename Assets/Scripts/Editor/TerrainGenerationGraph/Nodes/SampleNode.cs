using System;
using Editor.TerrainGenerationGraph.Nodes.NodeComponents;
using TerrainGenerationGraph.Scripts;
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

        public override TgGraph.TgTreeDto ToTgtNode(TgGraph.TgTreeDto tgTreeDto)
        {
            return _inputPort.GetNextTgtNodeDto(tgTreeDto);
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