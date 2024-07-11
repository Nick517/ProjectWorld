using System.Collections.Generic;
using static Editor.TerrainGenerationGraph.Nodes.NodeComponents.TggPort;
using static NodeOperations;

namespace Editor.TerrainGenerationGraph.Nodes
{
    public class AddNode : TggNode
    {
        protected override List<NodeType> NodeTypes => new() { NodeType.Add };
        

        protected override void SetUp()
        {
            title = "Add";

            AddInputPort("A");
            AddInputPort("B");
            AddOutputPort();
        }

        public override void Update()
        {
            var lowest = GetLowestDimension(ConnectedOutputPorts);
            SetAllPortDimensions(lowest);
            base.Update();
        }
    }
}