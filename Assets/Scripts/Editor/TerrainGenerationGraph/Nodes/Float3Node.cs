using static NodeOperations;

namespace Editor.TerrainGenerationGraph.Nodes
{
    public class Float3Node : TggNode
    {
        protected override void SetUp()
        {
            title = "Float 3";

            AddInputPort("X");
            AddInputPort("Y");
            AddInputPort("Z");
            AddOutputPort(NodeType.Float3, 3);
        }
    }
}