using System.Collections.Generic;
using static NodeOperations;

namespace Editor.TerrainGenerationGraph.Nodes
{
    public class Float3Node : TggNode
    {
        protected override List<NodeType> NodeTypes => new() { NodeType.Float3 };

        protected override void SetUp()
        {
            title = "Float 3";

            AddInputPort("X");
            AddInputPort("Y");
            AddInputPort("Z");
            AddOutputPort("Out", 3);
        }
    }
}