using System;
using System.Collections.Generic;
using Editor.TerrainGenerationGraph.Nodes.NodeComponents;
using static NodeOperations;

namespace Editor.TerrainGenerationGraph.Nodes
{
    public class Float2Node : TggNode, ITggNodeSerializable
    {
        #region Fields

        private InputPort _inputPortX;
        private InputPort _inputPortY;
        private OutputPort _outputPort;

        protected override List<NodeType> NodeTypes => new() { NodeType.Float2 };

        #endregion

        #region Methods

        protected override void SetUp()
        {
            title = "Float 2";

            _inputPortX = AddInputPort("X");
            _inputPortY = AddInputPort("Y");
            _outputPort = AddOutputPort("Out", 2);
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

            public Vector2NodeDto(Float2Node float2Node) : base(float2Node)
            {
                inputPortXDto = float2Node._inputPortX.ToDto();
                inputPortYDto = float2Node._inputPortY.ToDto();
                outputPortDto = float2Node._outputPort.ToDto();
            }

            public override TggNode Deserialize(TerrainGenGraphView graphView)
            {
                var vector2Node = (Float2Node)Create(graphView, typeof(Float2Node));

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