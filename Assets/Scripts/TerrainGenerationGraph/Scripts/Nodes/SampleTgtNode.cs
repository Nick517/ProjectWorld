using System;

namespace TerrainGenerationGraph.Scripts.Nodes
{
    [Serializable]
    public class SampleTgtNode : TgtNode
    {
        public TgtNode inputNode;

        public override float Traverse()
        {
            return inputNode.Traverse();
        }
    }
}