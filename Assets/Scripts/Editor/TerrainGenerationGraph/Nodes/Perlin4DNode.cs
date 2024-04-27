using System;
using ECS.Components;
using Editor.TerrainGenerationGraph.Nodes.NodeComponents;
using TerrainGenerationGraph.Scripts;

namespace Editor.TerrainGenerationGraph.Nodes
{
    public class Perlin4DNode : TggNode, ITggNodeSerializable
    {
        #region Fields

        private OutputPort _outputPort;

        #endregion

        #region Methods

        protected override void SetUp()
        {
            title = "Perlin 4D";

            _outputPort = AddOutputPort("Out", 4);
        }

        #endregion

        #region Terrain Generation Tree

        public override TgGraph.TgTreeDto ToTgtNode(TgGraph.TgTreeDto tgTreeDto)
        {
            var dto = new TgtNodeDto(TgGraphData.NodeType.Perlin4D);

            tgTreeDto.nodes.Add(dto);

            return tgTreeDto;
        }

        #endregion

        #region Serialization

        public Dto ToDto()
        {
            return new Perlin4DNodeDto(this);
        }

        [Serializable]
        public class Perlin4DNodeDto : Dto
        {
            public OutputPort.Dto outputPortDto;

            public Perlin4DNodeDto()
            {
            }

            public Perlin4DNodeDto(Perlin4DNode perlin4DNode) : base(perlin4DNode)
            {
                outputPortDto = perlin4DNode._outputPort.ToDto();
            }

            public override TggNode Deserialize(TerrainGenGraphView graphView)
            {
                var perlin4DNode = (Perlin4DNode)Create(graphView, typeof(Perlin4DNode));

                DeserializeTo(perlin4DNode);
                outputPortDto.DeserializeTo(perlin4DNode._outputPort);

                return perlin4DNode;
            }
        }

        #endregion
    }
}