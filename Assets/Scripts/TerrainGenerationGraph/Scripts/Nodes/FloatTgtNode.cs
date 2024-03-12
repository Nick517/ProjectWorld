using System;

namespace TerrainGenerationGraph.Scripts.Nodes
{
    [Serializable]
    public class FloatTgtNode : TgtNode
    {
        public TgtNode nextNode;

        public override float Traverse()
        {
            return nextNode.Traverse();
        }
    }
}