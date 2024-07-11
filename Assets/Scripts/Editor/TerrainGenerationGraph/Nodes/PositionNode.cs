using System.Collections.Generic;
using static NodeOperations;

namespace Editor.TerrainGenerationGraph.Nodes
{
    public class PositionNode : TggNode
    {
        protected override List<NodeType> NodeTypes => new() { NodeType.Position };

        protected override void SetUp()
        {
            title = "Position";

            AddOutputPort("Out", 3);
        }
    }
}