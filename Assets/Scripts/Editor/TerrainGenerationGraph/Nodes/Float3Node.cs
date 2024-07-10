using System;
using System.Collections.Generic;
using Editor.TerrainGenerationGraph.Nodes.NodeComponents;
using static NodeOperations;

namespace Editor.TerrainGenerationGraph.Nodes
{
    public class Float3Node : TggNode, ITggNodeSerializable
    {
        #region Fields

        private InputPort _inputPortX;
        private InputPort _inputPortY;
        private InputPort _inputPortZ;
        private OutputPort _outputPort;

        protected override List<NodeType> NodeTypes => new() { NodeType.Float3 };
        
        #endregion

        #region Methods

        protected override void SetUp()
        {
            title = "Float 3";

            _inputPortX = AddInputPort("X");
            _inputPortY = AddInputPort("Y");
            _inputPortZ = AddInputPort("Z");
            _outputPort = AddOutputPort("Out", 3);
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

            public Vector3NodeDto(Float3Node float3Node) : base(float3Node)
            {
                inputPortXDto = float3Node._inputPortX.ToDto();
                inputPortYDto = float3Node._inputPortY.ToDto();
                inputPortZDto = float3Node._inputPortZ.ToDto();
                outputPortDto = float3Node._outputPort.ToDto();
            }

            public override TggNode Deserialize(TerrainGenGraphView graphView)
            {
                var vector3Node = (Float3Node)Create(graphView, typeof(Float3Node));

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