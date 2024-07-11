using UnityEditor.Experimental.GraphView;

namespace Editor.TerrainGenerationGraph.Nodes
{
    public class SampleNode : TggNode
    {
        protected override void SetUp()
        {
            title = "Sample";

            AddInputPort();

            capabilities &= ~Capabilities.Deletable;
        }
    }
}