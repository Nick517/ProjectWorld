using System;
using ECS.Components;
using Editor.TerrainGenerationGraph.Nodes.NodeComponents;
using TerrainGenerationGraph.Scripts;

namespace Editor.TerrainGenerationGraph.Nodes
{
    public class Vector3Node : TggNode, ITggNodeSerializable
    {
        #region Fields

        private InputPort _inputPortX;
        private InputPort _inputPortY;
        private InputPort _inputPortZ;
        private OutputPort _outputPort;

        #endregion

        #region Methods

        protected override void SetUp()
        {
            title = "Vector 3";

            _inputPortX = AddInputPort("X");
            _inputPortY = AddInputPort("Y");
            _inputPortZ = AddInputPort("Z");
            _outputPort = AddOutputPort("Out", 3);
        }

        #endregion

        #region Terrain Genereration Tree

        public override TgGraph.TgTreeDto ToTgtNode(TgGraph.TgTreeDto tgTreeDto)
        {
            var dto = new TgtNodeDto(TgGraphData.NodeType.Float3);

            tgTreeDto.nodes.Add(dto);

            dto.nextIndexes.x = tgTreeDto.nodes.Count;
            tgTreeDto = _inputPortX.GetNextTgtNodeDto(tgTreeDto);

            dto.nextIndexes.y = tgTreeDto.nodes.Count;
            tgTreeDto = _inputPortY.GetNextTgtNodeDto(tgTreeDto);

            dto.nextIndexes.z = tgTreeDto.nodes.Count;
            tgTreeDto = _inputPortZ.GetNextTgtNodeDto(tgTreeDto);

            return tgTreeDto;
        }

        #endregion

        #region Serialization

        public Dto ToDto()
        {
            return new Vector3NodeDto(this);
        }

        [Serializable]
        public class Vector3NodeDto : Dto
        {
            public InputPort.Dto inputPortXDto;
            public InputPort.Dto inputPortYDto;
            public InputPort.Dto inputPortZDto;
            public OutputPort.Dto outputPortDto;

            public Vector3NodeDto()
            {
            }

            public Vector3NodeDto(Vector3Node vector3Node) : base(vector3Node)
            {
                inputPortXDto = vector3Node._inputPortX.ToDto();
                inputPortYDto = vector3Node._inputPortY.ToDto();
                inputPortZDto = vector3Node._inputPortZ.ToDto();
                outputPortDto = vector3Node._outputPort.ToDto();
            }

            public override TggNode Deserialize(TerrainGenGraphView graphView)
            {
                var vector3Node = (Vector3Node)Create(graphView, typeof(Vector3Node));

                DeserializeTo(vector3Node);
                inputPortXDto.DeserializeTo(vector3Node._inputPortX);
                inputPortYDto.DeserializeTo(vector3Node._inputPortY);
                inputPortZDto.DeserializeTo(vector3Node._inputPortZ);
                outputPortDto.DeserializeTo(vector3Node._outputPort);

                return vector3Node;
            }
        }

        #endregion
    }
}