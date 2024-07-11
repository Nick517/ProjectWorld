using System.Collections.Generic;
using static NodeOperations;

namespace Editor.TerrainGenerationGraph.Nodes
{
    public class Float4Node : TggNode
    {
        protected override List<NodeType> NodeTypes => new() { NodeType.Float4 };

        protected override void SetUp()
        {
            title = "Float 4";

            AddInputPort("X");
            AddInputPort("Y");
            AddInputPort("Z");
            AddInputPort("W");
            AddOutputPort("Out", 4);
        }
    }
}