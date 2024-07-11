using static NodeOperations;

namespace Editor.TerrainGenerationGraph.Nodes
{
    public class Float4Node : TggNode
    {
        protected override void SetUp()
        {
            title = "Float 4";

            AddInputPort("X");
            AddInputPort("Y");
            AddInputPort("Z");
            AddInputPort("W");
            AddOutputPort(NodeType.Float4, 4);
        }
    }
}