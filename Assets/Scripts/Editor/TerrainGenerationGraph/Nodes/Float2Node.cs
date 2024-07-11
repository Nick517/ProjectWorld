using System.Collections.Generic;
using static NodeOperations;

namespace Editor.TerrainGenerationGraph.Nodes
{
    public class Float2Node : TggNode
    {
        protected override List<NodeType> NodeTypes => new() { NodeType.Float2 };

        protected override void SetUp()
        {
            title = "Float 2";

            AddInputPort("X");
            AddInputPort("Y");
            AddOutputPort("Out", 2);
        }
    }
}