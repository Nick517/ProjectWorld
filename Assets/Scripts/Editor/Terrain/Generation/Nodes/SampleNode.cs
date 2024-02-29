using System;
using UnityEngine;

namespace Editor.Terrain.Generation.Nodes
{
    public class SampleNode : TgNode
    {
        public TgPort inputPort;

        protected override void SetUp()
        {
            title = "Sample";

            inputPort = AddInputPort("In(1)", typeof(float));
        }

        public override Dto ToDto()
        {
            return new SampleNodeDto(this);
        }

        [Serializable]
        public class SampleNodeDto : Dto
        {
            public TgPort.Dto inputPort;

            public SampleNodeDto(SampleNode sampleNode) : base(sampleNode)
            {
                inputPort = sampleNode.inputPort.ToDto();
            }

            public override TgNode Deserialize(TgGraphView graph)
            {
                var sampleNode = (SampleNode)Create(graph, typeof(SampleNode));
                sampleNode.inputPort.id = inputPort.id;
                sampleNode.id = id;
                sampleNode.SetPosition(position.AsVector2());

                return sampleNode;
            }
        }
    }
}