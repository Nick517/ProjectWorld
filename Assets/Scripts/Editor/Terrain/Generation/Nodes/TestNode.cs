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
            public TgPort.Dto inputPort;
            public TgPort.Dto outputPort;

            public TestNodeDto(TestNode testNode) : base(testNode)
            {
                inputPort = testNode.inputPort.ToDto();
                outputPort = testNode.inputPort.ToDto();
            }

            public override TgNode Deserialize(TgGraphView graph)
            {
                var testNode = (TestNode)Create(graph, typeof(TestNode));
                testNode.inputPort.id = inputPort.id;
                testNode.outputPort.id = outputPort.id;
                testNode.id = id;
                testNode.SetPosition(position.AsVector2());

                return testNode;
            }
        }
    }
}