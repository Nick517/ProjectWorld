using static NodeOperations;

namespace Editor.TerrainGenerationGraph.Nodes
{
    public class AddNode : TggNode
    {
        protected override void SetUp()
        {
            title = "Add";

            AddInputPort("A");
            AddInputPort("B");
            AddOutputPort(NodeType.Add);

            SetPortsToLowest = true;
        }
    }
}