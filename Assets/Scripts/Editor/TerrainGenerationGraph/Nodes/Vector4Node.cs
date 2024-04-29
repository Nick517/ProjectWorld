using System;
using ECS.Components;
using Editor.TerrainGenerationGraph.Nodes.NodeComponents;
using TerrainGenerationGraph.Scripts;
using TgGraph = TerrainGenerationGraph.Scripts.TgGraph;

namespace Editor.TerrainGenerationGraph.Nodes
{
    public class Vector4Node : TggNode, ITggNodeSerializable
    {
        #region Fields

        private InputPort _inputPortX;
        private InputPort _inputPortY;
        private InputPort _inputPortZ;
        private InputPort _inputPortW;
        private OutputPort _outputPort;

        #endregion

        #region Methods

        protected override void SetUp()
        {
            title = "Vector 4";

            _inputPortX = AddInputPort("X");
            _inputPortY = AddInputPort("Y");
            _inputPortZ = AddInputPort("Z");
            _inputPortW = AddInputPort("W");
            _outputPort = AddOutputPort("Out", 4);
        }

        #endregion

        #region Terrain Genereration Tree

        public override TgGraph.TgTreeDto ToTgtNode(TgGraph.TgTreeDto tgTreeDto)
        {
            var dto = new TgtNodeDto(TgTreeData.NodeType.Float4);

            tgTreeDto.nodes.Add(dto);

            dto.nextIndexes.x = tgTreeDto.nodes.Count;
            tgTreeDto = _inputPortX.GetNextTgtNodeDto(tgTreeDto);

            dto.nextIndexes.y = tgTreeDto.nodes.Count;
            tgTreeDto = _inputPortY.GetNextTgtNodeDto(tgTreeDto);

            dto.nextIndexes.z = tgTreeDto.nodes.Count;
            tgTreeDto = _inputPortZ.GetNextTgtNodeDto(tgTreeDto);

            dto.nextIndexes.w = tgTreeDto.nodes.Count;
            tgTreeDto = _inputPortW.GetNextTgtNodeDto(tgTreeDto);

            return tgTreeDto;
        }

        #endregion

        #region Serialization

        public Dto ToDto()
        {
            return new Vector4NodeDto(this);
        }

        [Serializable]
        public class Vector4NodeDto : Dto
        {
            public InputPort.Dto inputPortXDto;
            public InputPort.Dto inputPortYDto;
            public InputPort.Dto inputPortZDto;
            public InputPort.Dto inputPortWDto;
            public OutputPort.Dto outputPortDto;

            public Vector4NodeDto()
            {
            }

            public Vector4NodeDto(Vector4Node vector4Node) : base(vector4Node)
            {
                inputPortXDto = vector4Node._inputPortX.ToDto();
                inputPortYDto = vector4Node._inputPortY.ToDto();
                inputPortZDto = vector4Node._inputPortZ.ToDto();
                inputPortWDto = vector4Node._inputPortW.ToDto();
                outputPortDto = vector4Node._outputPort.ToDto();
            }

            public override TggNode Deserialize(TerrainGenGraphView graphView)
            {
                var vector4Node = (Vector4Node)Create(graphView, typeof(Vector4Node));

                DeserializeTo(vector4Node);
                inputPortXDto.DeserializeTo(vector4Node._inputPortX);
                inputPortYDto.DeserializeTo(vector4Node._inputPortY);
                inputPortZDto.DeserializeTo(vector4Node._inputPortZ);
                inputPortWDto.DeserializeTo(vector4Node._inputPortW);
                outputPortDto.DeserializeTo(vector4Node._outputPort);

                return vector4Node;
            }
        }

        #endregion
    }
}