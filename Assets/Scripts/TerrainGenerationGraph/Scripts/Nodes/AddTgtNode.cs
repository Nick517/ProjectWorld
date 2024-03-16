using System;
using UnityEngine;

namespace TerrainGenerationGraph.Scripts.Nodes
{
    [Serializable]
    public class AddTgtNode : TgtNode
    {
        public TgtNode nextNodeA;
        public TgtNode nextNodeB;

        public override Vector4 Traverse()
        {
            return nextNodeA.Traverse() + nextNodeB.Traverse();
        }
    }
}