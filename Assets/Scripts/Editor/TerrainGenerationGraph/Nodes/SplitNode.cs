using System;
using System.Collections.Generic;
using System.Linq;
using Editor.TerrainGenerationGraph.Nodes.NodeComponents;
using TerrainGenerationGraph.Scripts;
using UnityEngine;
using static NodeOperations;

namespace Editor.TerrainGenerationGraph.Nodes
{
    public class SplitNode : TggNode, ITggNodeSerializable
    {
        #region Fields

        private InputPort _inputPort;
        private OutputPort _outputPortX;
        private OutputPort _outputPortY;
        private OutputPort _outputPortZ;
        private OutputPort _outputPortW;

        protected override List<NodeType> NodeTypes => new()
        {
            NodeType.SplitOutX,
            NodeType.SplitOutY,
            NodeType.SplitOutZ,
            NodeType.SplitOutW
        };

        #endregion

        #region Methods

        protected override void SetUp()
        {
            title = "Split";

            _inputPort = AddInputPort();
            _outputPortX = AddOutputPort("X");
            _outputPortY = AddOutputPort("Y");
            _outputPortZ = AddOutputPort("Z");
            _outputPortW = AddOutputPort("W");
        }

        #endregion

        #region Terrain Generation Tree

        private TgtNodeDto _savedDtoX;
        private TgtNodeDto _savedDtoY;
        private TgtNodeDto _savedDtoZ;
        private TgtNodeDto _savedDtoW;

        public override TgtNodeDto GatherDto(InputPort inputPort = default)
        {
            var index = OutputPorts.FindIndex(port => inputPort != null && inputPort.ConnectedTggPort == port);
            var nextNodes = InputPorts.Select(port => port.NextTgtNodeDto()).ToArray();

            if (index == 0)
            {
                if (_savedDtoX != null)
                {
                    _savedDtoX.cached = true;

                    return _savedDtoX;
                }

                _savedDtoX = new TgtNodeDto(NodeType.SplitOutX, nextNodes);
                return _savedDtoX;
            }

            if (index == 1)
            {
                if (_savedDtoY != null)
                {
                    _savedDtoY.cached = true;

                    return _savedDtoY;
                }

                _savedDtoY = new TgtNodeDto(NodeType.SplitOutY, nextNodes);
                return _savedDtoY;
            }

            if (index == 2)
            {
                if (_savedDtoZ != null)
                {
                    _savedDtoZ.cached = true;

                    return _savedDtoZ;
                }

                _savedDtoZ = new TgtNodeDto(NodeType.SplitOutZ, nextNodes);
                return _savedDtoZ;
            }

            if (index == 3)
            {
                if (_savedDtoW != null)
                {
                    _savedDtoW.cached = true;

                    return _savedDtoW;
                }

                _savedDtoW = new TgtNodeDto(NodeType.SplitOutW, nextNodes);
                return _savedDtoW;
            }

            return null;
        }
        
        public override void ClearDto()
        {
            _savedDtoX = null;
            _savedDtoY = null;
            _savedDtoZ = null;
            _savedDtoW = null;
        }

        #endregion

        #region Serialization

        public Dto ToDto()
        {
            return new SplitNodeDto(this);
        }

        [Serializable]
        public class SplitNodeDto : Dto
        {
            public InputPort.Dto inputPortDto;
            public OutputPort.Dto outputPortXDto;
            public OutputPort.Dto outputPortYDto;
            public OutputPort.Dto outputPortZDto;
            public OutputPort.Dto outputPortWDto;

            public SplitNodeDto()
            {
            }

            public SplitNodeDto(SplitNode splitNode) : base(splitNode)
            {
                inputPortDto = splitNode._inputPort.ToDto();
                outputPortXDto = splitNode._outputPortX.ToDto();
                outputPortYDto = splitNode._outputPortY.ToDto();
                outputPortZDto = splitNode._outputPortZ.ToDto();
                outputPortWDto = splitNode._outputPortW.ToDto();
            }

            public override TggNode Deserialize(TerrainGenGraphView graphView)
            {
                var splitNode = (SplitNode)Create(graphView, typeof(SplitNode));

                DeserializeTo(splitNode);
                inputPortDto.DeserializeTo(splitNode._inputPort);
                outputPortXDto.DeserializeTo(splitNode._outputPortX);
                outputPortYDto.DeserializeTo(splitNode._outputPortY);
                outputPortZDto.DeserializeTo(splitNode._outputPortZ);
                outputPortWDto.DeserializeTo(splitNode._outputPortW);

                return splitNode;
            }
        }

        #endregion
    }
}