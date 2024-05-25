using System;
using Editor.TerrainGenerationGraph.Nodes.NodeComponents;
using static NodeOperations;

namespace Editor.TerrainGenerationGraph.Nodes
{
    public class FloatNode : TggNode, ITggNodeSerializable
    {
        #region Fields

        private InputPort _inputPort;
        private OutputPort _outputPort;

        #endregion

        #region Methods

        protected override void SetUp()
        {
            NodeType = NodeType.Skip;

            title = "Float";

            _inputPort = AddInputPort("X");
            _outputPort = AddOutputPort();
        }

        #endregion

        #region Serialization

        public Dto ToDto()
        {
            return new FloatNodeDto(this);
        }

        [Serializable]
        public class FloatNodeDto : Dto
        {
            public InputPort.Dto inputPortDto;
            public OutputPort.Dto outputPortDto;

            public FloatNodeDto()
            {
            }

            public FloatNodeDto(FloatNode floatNode) : base(floatNode)
            {
                inputPortDto = floatNode._inputPort.ToDto();
                outputPortDto = floatNode._outputPort.ToDto();
            }

            public override TggNode Deserialize(TerrainGenGraphView graphView)
            {
                var floatNode = (FloatNode)Create(graphView, typeof(FloatNode));

                DeserializeTo(floatNode);
                inputPortDto.DeserializeTo(floatNode._inputPort);
                outputPortDto.DeserializeTo(floatNode._outputPort);

                return floatNode;
            }
        }

        #endregion
    }
}