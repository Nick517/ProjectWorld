using System;

namespace Editor.Terrain.Generation.Nodes
{
    public class TestNode : TgNode
    {
        public TgPort inputPort;
        public TgPort outputPort;

        protected override void SetUp()
        {
            title = "Test";

            inputPort = AddInputPort("In(1)", typeof(float));
            outputPort = AddOutputPort("Out(1)", typeof(float));
        }
        
        public override Dto ToDto()
        {
            return new TestNodeDto(this);
        }

        [Serializable]
        public class TestNodeDto : Dto
        {
            public string inputPortId;
            public string outputPortId;

            public TestNodeDto(TestNode testNode) : base(testNode)
            {
                inputPortId = testNode.inputPort.id;
                outputPortId = testNode.outputPort.id;
            }

            public override TgNode Deserialize(TgGraphView graph)
            {
                var testNode = (TestNode)Create(graph, typeof(TestNode));
                testNode.inputPort.id = inputPortId;
                testNode.outputPort.id = outputPortId;
                testNode.id = id;
                testNode.SetPosition(position.AsVector2());

                return testNode;
            }
        }
    }
}