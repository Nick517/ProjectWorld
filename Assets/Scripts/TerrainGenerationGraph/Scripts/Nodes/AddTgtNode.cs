using System;

namespace TerrainGenerationGraph.Scripts.Nodes
{
    [Serializable]
    public class AddTgtNode : TgtNode
    {
        public TgtNode nextNodeA;
        public TgtNode nextNodeB;

        public override float Traverse()
        {
            return nextNodeA.Traverse() + nextNodeB.Traverse();
        }
    }
}