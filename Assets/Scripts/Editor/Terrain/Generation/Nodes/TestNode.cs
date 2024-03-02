using System;
using System.Collections.Generic;

namespace Editor.Terrain.Generation.Nodes
{
    public class TestNode : TgNode
    {
        #region Fields

        private TgPort _inputPort;
        private TgPort _outputPort;

        public override List<TgPort> TgPorts => new() { _inputPort, _outputPort };

        #endregion

        #region Methods

        protected override void SetUp()
        {
            title = "Test";

            _inputPort = AddInputPort("In(1)", typeof(float));
            _outputPort = AddOutputPort("Out(1)", typeof(float));
        }

        #endregion

        #region Serialization

        public override Dto ToDto()
        {
            return new TestNodeDto(this);
        }

        [Serializable]
        public class TestNodeDto : Dto
        {
            public string inputPortId;
            public string outputPortId;

            public TestNodeDto()
            {
            }

            public TestNodeDto(TestNode testNode) : base(testNode)
            {
                inputPortId = testNode._inputPort.id;
                outputPortId = testNode._outputPort.id;
            }

            public override TgNode Deserialize(TgGraph graph)
            {
                var testNode = (TestNode)Create(graph, typeof(TestNode));
                testNode._inputPort.id = inputPortId;
                testNode._outputPort.id = outputPortId;
                testNode.id = id;
                testNode.SetPosition(position.Deserialize());

                return testNode;
            }
        }

        #endregion
    }
}