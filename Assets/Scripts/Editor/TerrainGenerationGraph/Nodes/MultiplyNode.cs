using System;
using Editor.TerrainGenerationGraph.Nodes.NodeComponents;
using static NodeOperations;

namespace Editor.TerrainGenerationGraph.Nodes
{
    public class MultiplyNode : TggNode, ITggNodeSerializable
    {
        #region Fields

        private InputPort _inputPortA;
        private InputPort _inputPortB;
        private OutputPort _outputPort;

        #endregion

        #region Methods

        protected override void SetUp()
        {
            NodeType = NodeType.Multiply;

            title = "Multiply";

            _inputPortA = AddInputPort("A");
            _inputPortB = AddInputPort("B");
            _outputPort = AddOutputPort();
        }

        public override void Update()
        {
            var lowest = TggPort.GetLowestDimension(ConnectedOutputPorts);
            SetAllPortDimensions(lowest);
            base.Update();
        }

        #endregion

        #region Serialization

        public Dto ToDto()
        {
            return new MultiplyNodeDto(this);
        }

        [Serializable]
        public class MultiplyNodeDto : Dto
        {
            public InputPort.Dto inputPortADto;
            public InputPort.Dto inputPortBDto;
            public OutputPort.Dto outputPortDto;

            public MultiplyNodeDto()
            {
            }

            public MultiplyNodeDto(MultiplyNode multiplyNode) : base(multiplyNode)
            {
                inputPortADto = multiplyNode._inputPortA.ToDto();
                inputPortBDto = multiplyNode._inputPortB.ToDto();
                outputPortDto = multiplyNode._outputPort.ToDto();
            }

            public override TggNode Deserialize(TerrainGenGraphView graphView)
            {
                var multiplyNode = (MultiplyNode)Create(graphView, typeof(MultiplyNode));

                DeserializeTo(multiplyNode);
                inputPortADto.DeserializeTo(multiplyNode._inputPortA);
                inputPortBDto.DeserializeTo(multiplyNode._inputPortB);
                outputPortDto.DeserializeTo(multiplyNode._outputPort);

                return multiplyNode;
            }
        }

        #endregion
    }
}