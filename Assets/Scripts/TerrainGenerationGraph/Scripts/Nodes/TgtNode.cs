using System;

namespace TerrainGenerationGraph.Scripts.Nodes
{
    [Serializable]
    public abstract class TgtNode
    {
        public abstract float Traverse();
    }
}