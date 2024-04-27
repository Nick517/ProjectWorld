using System;
using ECS.Components;
using Editor.TerrainGenerationGraph.Nodes.NodeComponents;
using TerrainGenerationGraph.Scripts;

namespace Editor.TerrainGenerationGraph.Nodes
{
    public class Vector2Node : TggNode, ITggNodeSerializable
    {
        #region Fields

        private InputPort _inputPortX;
        private InputPort _inputPortY;
        private OutputPort _outputPort;

        #endregion

        #region Methods

        protected override void SetUp()
        {
            title = "Vector 2";

            _inputPortX = AddInputPort("X");
            _inputPortY = AddInputPort("Y");
            _outputPort = AddOutputPort("Out", 2);
        }

        #endregion

        #region Terrain Genereration Tree

        public override TgGraph.TgTreeDto ToTgtNode(TgGraph.TgTreeDto tgTreeDto)
        {
            var dto = new TgtNodeDto(TgGraphData.NodeType.Float2);

            tgTreeDto.nodes.Add(dto);

            dto.nextIndexes.x = tgTreeDto.nodes.Count;
            tgTreeDto = _inputPortX.GetNextTgtNodeDto(tgTreeDto);

            dto.nextIndexes.y = tgTreeDto.nodes.Count;
            tgTreeDto = _inputPortY.GetNextTgtNodeDto(tgTreeDto);

            return tgTreeDto;
        }

        #endregion

        #region Serialization

        public Dto ToDto()
        {
            return new Vector2NodeDto(this);
        }

        [Serializable]
        public class Vector2NodeDto : Dto
        {
            public InputPort.Dto inputPortXDto;
            public InputPort.Dto inputPortYDto;
            public OutputPort.Dto outputPortDto;

            public Vector2NodeDto()
            {
            }

            public Vector2NodeDto(Vector2Node vector2Node) : base(vector2Node)
            {
                inputPortXDto = vector2Node._inputPortX.ToDto();
                inputPortYDto = vector2Node._inputPortY.ToDto();
                outputPortDto = vector2Node._outputPort.ToDto();
            }

            public override TggNode Deserialize(TerrainGenGraphView graphView)
            {
                var vector2Node = (Vector2Node)Create(graphView, typeof(Vector2Node));

                DeserializeTo(vector2Node);
                inputPortXDto.DeserializeTo(vector2Node._inputPortX);
                inputPortYDto.DeserializeTo(vector2Node._inputPortY);
                outputPortDto.DeserializeTo(vector2Node._outputPort);

                return vector2Node;
            }
        }

        #endregion
    }
}