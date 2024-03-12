using System;

namespace TerrainGenerationGraph.Scripts.Nodes
{
    [Serializable]
    public class TgtDefaultValue : TgtNode
    {
        public float value;
        
        public override float Traverse()
        {
            return value;
        }
    }
}