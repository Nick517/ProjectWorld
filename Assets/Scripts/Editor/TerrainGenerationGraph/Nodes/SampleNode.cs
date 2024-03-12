using System;
using System.Collections.Generic;
using Editor.TerrainGenerationGraph.Nodes.NodeComponents;
using TerrainGenerationGraph.Scripts.Nodes;
using UnityEditor.Experimental.GraphView;

namespace Editor.TerrainGenerationGraph.Nodes
{
    public class SampleNode : TggNode
    {
        #region Fields

        private TggPort _inputPort;

        public override List<TggPort> TggPorts => new() { _inputPort };

        #endregion

        #region Methods

        protected override void SetUp()
        {
            title = "Sample";

            _inputPort = AddInputPort("In(1)", typeof(float));

            capabilities &= ~Capabilities.Deletable;
        }

        public override TgtNode ToTgtNode()
        {
            return new SampleTgtNode
            {
                nextNode = _inputPort.ConnectedTgtNode
            };
        }

        #endregion

        #region Serialization

        public override Dto ToDto()
        {
            return new SampleNodeDto(this);
        }

        [Serializable]
        public class SampleNodeDto : Dto
        {
            public string inputPortId;

            public SampleNodeDto()
            {
            }

            public SampleNodeDto(SampleNode sampleNode) : base(sampleNode)
            {
                inputPortId = sampleNode._inputPort.id;
            }

            public override TggNode Deserialize(TerrainGenGraphView graphView)
            {
                var sampleNode = (SampleNode)Create(graphView, typeof(SampleNode));
                sampleNode._inputPort.id = inputPortId;
                sampleNode.id = id;
                sampleNode.Position = position.Deserialize();

                return sampleNode;
            }
        }

        #endregion
    }
}