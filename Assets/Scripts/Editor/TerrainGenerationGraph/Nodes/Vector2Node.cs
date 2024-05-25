using System;
using Editor.TerrainGenerationGraph.Nodes.NodeComponents;
using static NodeOperations;

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
            NodeType = NodeType.Float2;

            title = "Vector 2";

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