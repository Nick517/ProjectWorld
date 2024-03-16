using System;
using UnityEngine;

namespace TerrainGenerationGraph.Scripts.Nodes
{
    [Serializable]
    public class FloatTgtNode : TgtNode
    {
        public TgtNode nextNode;

        public override Vector4 Traverse()
        {
            return nextNode.Traverse();
        }
    }
}