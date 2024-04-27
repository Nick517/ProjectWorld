using System;
using Editor.TerrainGenerationGraph.Nodes.NodeComponents;
using TerrainGenerationGraph.Scripts;

namespace Editor.TerrainGenerationGraph.Nodes
{
    public class SplitNode : TggNode, ITggNodeSerializable
    {
        #region Fields

        private InputPort _inputPort;
        private OutputPort _outputPortX;
        private OutputPort _outputPortY;
        private OutputPort _outputPortZ;
        private OutputPort _outputPortW;

        #endregion

        #region Methods

        protected override void SetUp()
        {
            title = "Split";

            _inputPort = AddInputPort();
            _outputPortX = AddOutputPort("X");
            _outputPortY = AddOutputPort("Y");
            _outputPortZ = AddOutputPort("Z");
            _outputPortW = AddOutputPort("W");
        }

        #endregion

        #region Terrain Genereration Tree

        public override TgGraph.TgTreeDto ToTgtNode(TgGraph.TgTreeDto tgTreeDto)
        {
            return _inputPort.GetNextTgtNodeDto(tgTreeDto);
        }

        #endregion

        #region Serialization

        public Dto ToDto()
        {
            return new SplitNodeDto(this);
        }

        [Serializable]
        public class SplitNodeDto : Dto
        {
            public InputPort.Dto inputPortDto;
            public OutputPort.Dto outputPortXDto;
            public OutputPort.Dto outputPortYDto;
            public OutputPort.Dto outputPortZDto;
            public OutputPort.Dto outputPortWDto;

            public SplitNodeDto()
            {
            }

            public SplitNodeDto(SplitNode splitNode) : base(splitNode)
            {
                inputPortDto = splitNode._inputPort.ToDto();
                outputPortXDto = splitNode._outputPortX.ToDto();
                outputPortYDto = splitNode._outputPortY.ToDto();
                outputPortZDto = splitNode._outputPortZ.ToDto();
                outputPortWDto = splitNode._outputPortW.ToDto();
            }

            public override TggNode Deserialize(TerrainGenGraphView graphView)
            {
                var splitNode = (SplitNode)Create(graphView, typeof(SplitNode));

                DeserializeTo(splitNode);
                inputPortDto.DeserializeTo(splitNode._inputPort);
                outputPortXDto.DeserializeTo(splitNode._outputPortX);
                outputPortYDto.DeserializeTo(splitNode._outputPortY);
                outputPortZDto.DeserializeTo(splitNode._outputPortZ);
                outputPortWDto.DeserializeTo(splitNode._outputPortW);

                return splitNode;
            }
        }

        #endregion
    }
}