using static NodeOperations;

namespace Editor.TerrainGenerationGraph.Nodes
{
    public class Float2Node : TggNode
    {
        protected override void SetUp()
        {
            title = "Float 2";

            AddInputPort("X");
            AddInputPort("Y");
            AddOutputPort(NodeType.Float2, 2);
        }
    }
}