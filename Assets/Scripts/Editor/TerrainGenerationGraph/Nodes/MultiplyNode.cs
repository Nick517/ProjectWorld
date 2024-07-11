using System.Collections.Generic;
using Editor.TerrainGenerationGraph.Nodes.NodeComponents;
using static NodeOperations;

namespace Editor.TerrainGenerationGraph.Nodes
{
    public class MultiplyNode : TggNode
    {
        protected override List<NodeType> NodeTypes => new() { NodeType.Multiply };


        protected override void SetUp()
        {
            title = "Multiply";

            AddInputPort("A");
            AddInputPort("B");
            AddOutputPort();
        }

        public override void Update()
        {
            var lowest = TggPort.GetLowestDimension(ConnectedOutputPorts);
            SetAllPortDimensions(lowest);
            base.Update();
        }
    }
}