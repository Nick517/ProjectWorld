using System;
using System.Collections.Generic;
using Editor.TerrainGenerationGraph.Nodes.NodeComponents;
using static NodeOperations;

namespace Editor.TerrainGenerationGraph.Nodes
{
    public class Perlin3DNode : TggNode, ITggNodeSerializable
    {
        #region Fields

        private InputPort _inputCoordPort;
        private InputPort _inputScalePort;
        private OutputPort _outputPort;

        protected override List<NodeType> NodeTypes => new() { NodeType.Perlin3D };

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