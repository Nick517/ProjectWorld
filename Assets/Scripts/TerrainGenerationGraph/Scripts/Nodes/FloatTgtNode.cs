using System;

namespace TerrainGenerationGraph.Scripts.Nodes
{
    [Serializable]
    public class FloatTgtNode : TgtNode
    {
        public float value;

        public override float Traverse()
        {
            return value;
        }
    }
}