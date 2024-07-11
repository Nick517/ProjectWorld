using System.Collections.Generic;
using static NodeOperations;

namespace Editor.TerrainGenerationGraph.Nodes
{
    public class SplitNode : TggNode
    {
        protected override List<NodeType> NodeTypes => new()
        {
            NodeType.SplitOutX,
            NodeType.SplitOutY,
            NodeType.SplitOutZ,
            NodeType.SplitOutW
        };

        protected override void SetUp()
        {
            title = "Split";

            AddInputPort();
            AddOutputPort("X");
            AddOutputPort("Y");
            AddOutputPort("Z");
            AddOutputPort("W");
        }
    }
}