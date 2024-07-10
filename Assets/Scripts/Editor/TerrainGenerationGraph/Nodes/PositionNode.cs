using System;
using System.Collections.Generic;
using Editor.TerrainGenerationGraph.Nodes.NodeComponents;
using static NodeOperations;

namespace Editor.TerrainGenerationGraph.Nodes
{
    public class PositionNode : TggNode, ITggNodeSerializable
    {
        #region Fields

        private OutputPort _outputPort;
        
        protected override List<NodeType> NodeTypes => new() { NodeType.Position };

        #endregion

        #region Methods

        protected override void SetUp()
        {
            title = "Position";

            _outputPort = AddOutputPort("Out", 3);
        }

        #endregion

        #region Serialization

        public Dto ToDto()
        {
            return new PositionNodeDto(this);
        }

        [Serializable]
        public class PositionNodeDto : Dto
        {
            public OutputPort.Dto outputPortDto;

            public PositionNodeDto()
            {
            }

            public PositionNodeDto(PositionNode positionNode) : base(positionNode)
            {
                outputPortDto = positionNode._outputPort.ToDto();
            }

            public override TggNode Deserialize(TerrainGenGraphView graphView)
            {
                var positionNode = (PositionNode)Create(graphView, typeof(PositionNode));

                DeserializeTo(positionNode);
                outputPortDto.DeserializeTo(positionNode._outputPort);

                return positionNode;
            }
        }

        #endregion
    }
}