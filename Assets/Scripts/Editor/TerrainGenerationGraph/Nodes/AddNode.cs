using System;
using System.Collections.Generic;
using Editor.TerrainGenerationGraph.Nodes.NodeComponents;
using TerrainGenerationGraph.Scripts.Nodes;

namespace Editor.TerrainGenerationGraph.Nodes
{
    public class AddNode : TggNode
    {
        #region Fields

        private TggPort _inputPortA;
        private TggPort _inputPortB;
        private TggPort _outputPort;

        public override List<TggPort> TggPorts => new() { _inputPortA, _inputPortB, _outputPort };

        #endregion

        #region Methods

        protected override void SetUp()
        {
            title = "Add";

            _inputPortA = AddInputPort("A(1)", typeof(float));
            _inputPortB = AddInputPort("B(1)", typeof(float));
            _outputPort = AddOutputPort("Out(1)", typeof(float));
        }

        public override TgtNode ToTgtNode()
        {
            return new AddTgtNode
            {
                inputNodeA = _inputPortA.GetConnectedTgtNode(),
                inputNodeB = _inputPortB.GetConnectedTgtNode()
            };
        }

        #endregion

        #region Serialization

        public override Dto ToDto()
        {
            return new AddNodeDto(this);
        }

        [Serializable]
        public class AddNodeDto : Dto
        {
            public string inputPortAId;
            public string inputPortBId;
            public string outputPortId;

            public AddNodeDto()
            {
            }

            public AddNodeDto(AddNode addNode) : base(addNode)
            {
                inputPortAId = addNode._inputPortA.id;
                inputPortBId = addNode._inputPortB.id;
                outputPortId = addNode._outputPort.id;
            }

            public override TggNode Deserialize(TerrainGenGraphView graphView)
            {
                var addNode = (AddNode)Create(graphView, typeof(AddNode));
                addNode._inputPortA.id = inputPortAId;
                addNode._inputPortB.id = inputPortBId;
                addNode._outputPort.id = outputPortId;
                addNode.id = id;
                addNode.SetPosition(position.Deserialize());

                return addNode;
            }
        }

        #endregion
    }
}