using System;
using System.Collections.Generic;
using Editor.TerrainGenerationGraph.Nodes.NodeComponents;
using static NodeOperations;

namespace Editor.TerrainGenerationGraph.Nodes
{
    public class Float4Node : TggNode, ITggNodeSerializable
    {
        #region Fields

        private InputPort _inputPortX;
        private InputPort _inputPortY;
        private InputPort _inputPortZ;
        private InputPort _inputPortW;
        private OutputPort _outputPort;

        protected override List<NodeType> NodeTypes => new() { NodeType.Float4 };

        #endregion

        #region Methods

        protected override void SetUp()
        {
            title = "Float 4";

            _inputPortX = AddInputPort("X");
            _inputPortY = AddInputPort("Y");
            _inputPortZ = AddInputPort("Z");
            _inputPortW = AddInputPort("W");
            _outputPort = AddOutputPort("Out", 4);
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

            public Vector4NodeDto(Float4Node float4Node) : base(float4Node)
            {
                inputPortXDto = float4Node._inputPortX.ToDto();
                inputPortYDto = float4Node._inputPortY.ToDto();
                inputPortZDto = float4Node._inputPortZ.ToDto();
                inputPortWDto = float4Node._inputPortW.ToDto();
                outputPortDto = float4Node._outputPort.ToDto();
            }

            public override TggNode Deserialize(TerrainGenGraphView graphView)
            {
                var vector4Node = (Float4Node)Create(graphView, typeof(Float4Node));

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