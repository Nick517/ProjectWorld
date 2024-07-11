using static NodeOperations;

namespace Editor.TerrainGenerationGraph.Nodes
{
    public class Perlin3DNode : TggNode
    {
        protected override void SetUp()
        {
            title = "Perlin 3D";

            AddInputPort("Coord", 3);
            AddInputPort("Scale");
            AddOutputPort(NodeType.Perlin3D);
        }
    }
}