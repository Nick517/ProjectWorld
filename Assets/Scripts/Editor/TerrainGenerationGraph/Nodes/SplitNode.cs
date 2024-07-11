using static NodeOperations;

namespace Editor.TerrainGenerationGraph.Nodes
{
    public class SplitNode : TggNode
    {
        protected override void SetUp()
        {
            title = "Split";

            AddInputPort();
            AddOutputPort("X", NodeType.SplitOutX);
            AddOutputPort("Y", NodeType.SplitOutY);
            AddOutputPort("Z", NodeType.SplitOutZ);
            AddOutputPort("W", NodeType.SplitOutW);
        }
    }
}