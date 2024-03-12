using System;
using System.Collections.Generic;
using Editor.TerrainGenerationGraph.Nodes.NodeComponents;
using TerrainGenerationGraph.Scripts.Nodes;

namespace Editor.TerrainGenerationGraph.Nodes
{
    public class FloatNode : TggNode
    {
        #region Fields

        private TggPort _inputPort;
        private TggPort _outputPort;

        public override List<TggPort> TggPorts => new() { _inputPort, _outputPort };

        #endregion

        #region Methods

        protected override void SetUp()
        {
            title = "Float";

            _inputPort = AddInputPort("In(1)", typeof(float));
            _outputPort = AddOutputPort("Out(1)", typeof(float));
        }

        #endregion

        #region Terrain Genereration Tree

        public override TgtNode ToTgtNode()
        {
            return new FloatTgtNode
            {
                nextNode = _inputPort.ConnectedTgtNode
            };
        }

        #endregion

        #region Serialization

        public override Dto ToDto()
        {
            return new FloatNodeDto(this);
        }

        [Serializable]
        public class FloatNodeDto : Dto
        {
            public string inputPortId;
            public string outputPortId;

            public FloatNodeDto()
            {
            }

            public FloatNodeDto(FloatNode floatNode) : base(floatNode)
            {
                inputPortId = floatNode._inputPort.id;
                outputPortId = floatNode._outputPort.id;
            }

            public override TggNode Deserialize(TerrainGenGraphView graphView)
            {
                var floatNode = (FloatNode)Create(graphView, typeof(FloatNode));
                floatNode._inputPort.id = inputPortId;
                floatNode._outputPort.id = outputPortId;
                floatNode.id = id;
                floatNode.Position = position.Deserialize();

                return floatNode;
            }
        }

        #endregion
    }
}