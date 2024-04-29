using System;
using ECS.Components;
using Editor.TerrainGenerationGraph.Nodes.NodeComponents;
using TerrainGenerationGraph.Scripts;
using TgGraph = TerrainGenerationGraph.Scripts.TgGraph;

namespace Editor.TerrainGenerationGraph.Nodes
{
    public class Perlin3DNode : TggNode, ITggNodeSerializable
    {
        #region Fields

        private InputPort _inputCoordPort;
        private InputPort _inputScalePort;

        private OutputPort _outputPort;

        #endregion

        #region Methods

        protected override void SetUp()
        {
            title = "Perlin 3D";

            _inputCoordPort = AddInputPort("Coord", 3);
            _inputScalePort = AddInputPort("Scale");
            _outputPort = AddOutputPort();
        }

        #endregion

        #region Terrain Generation Tree

        public override TgGraph.TgTreeDto ToTgtNode(TgGraph.TgTreeDto tgTreeDto)
        {
            var dto = new TgtNodeDto(TgTreeData.NodeType.Perlin3D);

            tgTreeDto.nodes.Add(dto);

            dto.nextIndexes.x = tgTreeDto.nodes.Count;
            tgTreeDto = _inputCoordPort.GetNextTgtNodeDto(tgTreeDto);

            dto.nextIndexes.y = tgTreeDto.nodes.Count;
            tgTreeDto = _inputScalePort.GetNextTgtNodeDto(tgTreeDto);

            return tgTreeDto;
        }

        #endregion

        #region Serialization

        public Dto ToDto()
        {
            return new Perlin3DNodeDto(this);
        }

        [Serializable]
        public class Perlin3DNodeDto : Dto
        {
            public InputPort.Dto inputCoordDto;
            public InputPort.Dto inputScaleDto;
            public OutputPort.Dto outputPortDto;

            public Perlin3DNodeDto()
            {
            }

            public Perlin3DNodeDto(Perlin3DNode perlin3DNode) : base(perlin3DNode)
            {
                inputCoordDto = perlin3DNode._inputCoordPort.ToDto();
                inputScaleDto = perlin3DNode._inputScalePort.ToDto();
                outputPortDto = perlin3DNode._outputPort.ToDto();
            }

            public override TggNode Deserialize(TerrainGenGraphView graphView)
            {
                var perlin3DNode = (Perlin3DNode)Create(graphView, typeof(Perlin3DNode));

                DeserializeTo(perlin3DNode);
                inputCoordDto.DeserializeTo(perlin3DNode._inputCoordPort);
                inputScaleDto.DeserializeTo(perlin3DNode._inputScalePort);
                outputPortDto.DeserializeTo(perlin3DNode._outputPort);

                return perlin3DNode;
            }
        }

        #endregion
    }
}