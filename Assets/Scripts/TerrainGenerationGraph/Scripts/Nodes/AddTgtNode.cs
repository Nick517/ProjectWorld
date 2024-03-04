using System;

namespace TerrainGenerationGraph.Scripts.Nodes
{
    [Serializable]
    public class AddTgtNode : TgtNode
    {
        public TgtNode inputNodeA;
        public TgtNode inputNodeB;

        public override float Traverse()
        {
            return inputNodeA.Traverse() + inputNodeB.Traverse();
        }
    }
}