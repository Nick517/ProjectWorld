using System.Collections.Generic;
using static NodeOperations;

namespace Editor.TerrainGenerationGraph.Nodes
{
    public class Perlin3DNode : TggNode
    {
        protected override List<NodeType> NodeTypes => new() { NodeType.Perlin3D };

        protected override void SetUp()
        {
            title = "Perlin 3D";

            AddInputPort("Coord", 3);
            AddInputPort("Scale");
            AddOutputPort();
        }
    }
}