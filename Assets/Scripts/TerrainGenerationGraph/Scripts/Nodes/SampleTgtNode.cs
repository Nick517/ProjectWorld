using System;

namespace TerrainGenerationGraph.Scripts.Nodes
{
    [Serializable]
    public class SampleTgtNode : TgtNode
    {
        public TgtNode nextNode;

        public override float Traverse()
        {
            return nextNode.Traverse();
        }
    }
}