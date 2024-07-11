using static NodeOperations;

namespace Editor.TerrainGenerationGraph.Nodes
{
    public class PositionNode : TggNode
    {
        protected override void SetUp()
        {
            title = "Position";

            AddOutputPort(NodeType.Position, 3);
        }
    }
}