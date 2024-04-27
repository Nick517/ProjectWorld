using System;
using ECS.Components;
using Editor.TerrainGenerationGraph.Nodes.NodeComponents;
using TerrainGenerationGraph.Scripts;

namespace Editor.TerrainGenerationGraph.Nodes
{
    public class AddNode : TggNode, ITggNodeSerializable
    {
        #region Fields

        private InputPort _inputPortA;
        private InputPort _inputPortB;
        private OutputPort _outputPort;

        #endregion

        #region Methods

        protected override void SetUp()
        {
            title = "Add";

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
            var dto = new TgtNodeDto(TgGraphData.NodeType.Add);

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
            return new AddNodeDto(this);
        }

        [Serializable]
        public class AddNodeDto : Dto
        {
            public InputPort.Dto inputPortADto;
            public InputPort.Dto inputPortBDto;
            public OutputPort.Dto outputPortDto;

            public AddNodeDto()
            {
            }

            public AddNodeDto(AddNode addNode) : base(addNode)
            {
                inputPortADto = addNode._inputPortA.ToDto();
                inputPortBDto = addNode._inputPortB.ToDto();
                outputPortDto = addNode._outputPort.ToDto();
            }

            public override TggNode Deserialize(TerrainGenGraphView graphView)
            {
                var addNode = (AddNode)Create(graphView, typeof(AddNode));

                DeserializeTo(addNode);
                inputPortADto.DeserializeTo(addNode._inputPortA);
                inputPortBDto.DeserializeTo(addNode._inputPortB);
                outputPortDto.DeserializeTo(addNode._outputPort);

                return addNode;
            }
        }

        #endregion
    }
}