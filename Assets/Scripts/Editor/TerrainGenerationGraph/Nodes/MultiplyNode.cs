using static NodeOperations;

namespace Editor.TerrainGenerationGraph.Nodes
{
    public class MultiplyNode : TggNode
    {
        protected override void SetUp()
        {
            title = "Multiply";

            AddInputPort("A");
            AddInputPort("B");
            AddOutputPort(NodeType.Multiply);

            SetPortsToLowest = true;
        }
    }
}