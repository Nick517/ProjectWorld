using System;
using ECS.Components;
using Editor.TerrainGenerationGraph.Nodes.NodeComponents;
using TerrainGenerationGraph.Scripts;
using TgGraph = TerrainGenerationGraph.Scripts.TgGraph;

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

        #region Terrain Generation Tree

        public override TgGraph.TgTreeDto ToTgtNode(TgGraph.TgTreeDto tgTreeDto)
        {
            var dto = new TgtNodeDto(TgTreeData.NodeType.Multiply);

            tgTreeDto.nodes.Add(dto);

            dto.nextIndexes.x = tgTreeDto.nodes.Count;
            tgTreeDto = _inputPortA.GetNextTgtNodeDto(tgTreeDto);

            dto.nextIndexes.y = tgTreeDto.nodes.Count;
            tgTreeDto = _inputPortB.GetNextTgtNodeDto(tgTreeDto);

            return tgTreeDto;
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